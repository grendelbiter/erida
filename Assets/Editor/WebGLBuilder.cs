// in the Editor folder within Assets.
using UnityEditor;


//to be used on the command line:
//$ Unity -quit -batchmode -executeMethod WebGLBuilder.build

class WebGLBuilder
{
    static void build()
    {
        string[] scenes = { "Assets/Scenes/Launcher 1.unity", "Assets/Scenes/Playground.unity" };
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = @"E:\GitRepos\WulfridaWebsite\updater\versions\Build\webgl";
        options.target = BuildTarget.WebGL;
        options.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(options);
    }
}
