using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace Unity.Trimmer.Cli.Commands
{
    public static class Trim
    {
        public static void Execute(string input, string output, string classdata, string font)
        {
            var manager = new AssetsManager(); 
            var asset = manager.LoadAssetsFile(input, false);

            if (!string.IsNullOrEmpty(classdata))
                manager.LoadClassPackage(classdata); // TODO: Is this necessary?
            
            manager.LoadClassDatabaseFromPackage(asset.file.typeTree.unityVersion);
            
            Console.WriteLine($"Asset found: {asset.name} ({asset.file.typeTree.unityVersion})");

            byte[] fontBytes = null;
            if (!string.IsNullOrEmpty(font))
                fontBytes = File.ReadAllBytes(font);
            
            var replacers = new List<AssetsReplacer>();
            var empty = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 2x2 32bit
            
            // Loop all assets to find all Texture2D and replace them with 2x2 empty one
            // TODO: Still left with ~350kb of crap, seems like there is a compute shader worth 200kb, not sure if worth trying to slim the rest, when compressed with brotli we're left with less than 66kb which is a huge gain compared to before
            foreach (var info in asset.table.assetFileInfo)
            {
                var baseField = manager.GetTypeInstance(asset, info).GetBaseField();
                var name = baseField.Get("m_Name").GetValue().AsString();

                Console.WriteLine($"Found asset: {name} / {(AssetClassID) info.curFileType}");
                
                // We've got a `Texture2D` so replace it
                if (info.curFileType == (int) AssetClassID.Texture2D)
                {
                    var texture = TextureFile.ReadTextureFile(baseField);
                    if (texture.m_Width < 2 && texture.m_Height < 2) continue;
                    
                    Console.WriteLine($"{texture.m_Width} / {texture.m_Height} / {(TextureFormat) texture.m_TextureFormat}");
                    
                    // Create the new texture
                    texture.m_TextureFormat = (int) TextureFormat.RGBA32;
                    var data = TextureFile.Encode(empty, (TextureFormat) texture.m_TextureFormat, 2, 2);
                    
                    texture.SetTextureDataRaw(data, 2, 2);
                    texture.WriteTo(baseField);
                    
                    var bytes = baseField.WriteToByteArray();
                    var replacer = new AssetsReplacerFromMemory(0, info.index, (int) info.curFileType, 0xffff, bytes);
                    replacers.Add(replacer);

                    Console.WriteLine($"*** Texture replaced!");
                }
                
                // Also replace "Arial" font since we don't need it
                if (fontBytes != null && info.curFileType == (int) AssetClassID.Font)
                {
                    Console.WriteLine($"Found Font!");
                    foreach (var child in baseField.children)
                    {
                        Console.WriteLine($"Child: {child.GetName()} / {child.GetFieldType()}");

                        if (child.GetName() == "m_FontData")
                        {
                            Console.WriteLine($"Data:");
                            foreach (var child2 in child.children)
                            {
                                Console.WriteLine($"Child2: {child2.GetName()} / {child2.GetFieldType()}");

                                if (child2.GetName() == "Array")
                                {
                                    // Extract font data
                                    /*var size = child2.GetValue().AsArray().size;
                                    var bytes = new byte[size];
                                    for (int i = 0; i < size; i++)
                                    {
                                        bytes[i] = (byte) child2[i].GetValue().AsInt();
                                    }
                                    
                                    File.WriteAllBytes($"{font}.copy", bytes);*/
                                    
                                    // Overwrite data
                                    child2.GetValue().Set(new AssetTypeArray(fontBytes.Length));

                                    AssetTypeValueField[] children = new AssetTypeValueField[fontBytes.Length];
                                    for (int i = 0; i < fontBytes.Length; i++)
                                    {
                                        AssetTypeValueField c = ValueBuilder.DefaultValueFieldFromArrayTemplate(child2);
                                        c.GetValue().Set(fontBytes[i]);
                                        children[i] = c;
                                    }

                                    child2.SetChildrenList(children);
                                    
                                    // Replace
                                    var bytes = baseField.WriteToByteArray();
                                    var replacer = new AssetsReplacerFromMemory(0, info.index, (int) info.curFileType, 0xffff, bytes);
                                    replacers.Add(replacer);
                                }
                            }
                        }
                    }
                }
            }
            
            Console.WriteLine($"Writing output to: \"{output}\"");
            
            File.Delete(output);
            using (var stream = File.OpenWrite(output))
            using (var writer = new AssetsFileWriter(stream))
                asset.file.Write(writer, 0, replacers, 0);

            Console.WriteLine($"Done!");
            
            manager.UnloadAllAssetsFiles();
        }
    }
}