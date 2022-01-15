using System;
using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace Unity.Trimmer.Cli.Commands
{
    public static class Trim
    {
        public static void Execute(string input, string output, string classdata)
        {
            var manager = new AssetsManager(); 
            var asset = manager.LoadAssetsFile(input, false);

            manager.LoadClassPackage(classdata); // TODO: Is this necessary?
            manager.LoadClassDatabaseFromPackage(asset.file.typeTree.unityVersion);
            
            Console.WriteLine($"Asset found: {asset.name} ({asset.file.typeTree.unityVersion})");

            var replacers = new List<AssetsReplacer>();
            var empty = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // 2x2 32bit
            
            // Loop all assets to find all Texture2D and replace them with 2x2 empty one
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
                if (info.curFileType == (int) AssetClassID.Font)
                {
                    // TODO: !!!
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