# Unity.Trimmer
Reduce size of unity asset bundled by default

Unity include a file by default called `unity_default_resources` which includes a bunch of unnecessary assets.

There is no easy way to remove it and it doesn't seems like the build process attempts to remove unnecessary assets from it either, so the file is simply copied as is... All 3.36MB (WebGL / 2021.1.28f1) of it... Which is kind of a lot for web games.

For example, a 2048x1024 2MB Texture called "UnitySplash-cube" is include in it, the default unity splash screen, even if you overwrite it in Project Setting.

It is very disappointing to see this, especially since the WebGL team at Unity is making great progress attempting to reduce the size of the code and yet they leave an unused 2MB Texture by default with no way to remove it.

This tools use [AssetsTools.NET](https://github.com/nesrak1/AssetsTools.NET) to read that file, replace all `Texture2D` inside of it and write a new trimmed copy of the file.

Replace that file in Unity Editor folder: `C:\Program Files\Unity\Hub\Editor\XXXXXXX\Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\data\unity_default_resources`

Build your project and notice how you reduce the output size by 3MB!

The file is also included in the "Extra" folder for those that don't want to build the program.