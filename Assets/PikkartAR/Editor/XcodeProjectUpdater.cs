#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using UnityEditor.iOS.Xcode;
using System.IO;

public class MyBuildPostprocessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            string target = proj.TargetGuidByName("Unity-iPhone");

            // Set a custom link flag
            proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-lz -lsqlite3");
			// Disable bitcode
			proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            File.WriteAllText(projPath, proj.WriteToString());
        }
    }
}
#endif
