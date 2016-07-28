using UnityEngine;
using UnityEditor;
using System.Collections;
using AncientLightStudios.uTomate;
using AncientLightStudios.uTomate.API;


[UTActionInfo(actionCategory = "FIE Build")]
[UTDoc(title = "SetBuildSystemConfiguration", description = "Sets the parameters of the build system configuration.")]
public class SetBuildSystemConfiguration : UTAction {

    [UTInspectorHint(required = true, order = 0)]
    [UTDoc(description = "A path to build into.")]
    public UTString OutputPath;

    [UTInspectorHint(required = true, order = 1)]
    [UTDoc(description = "Paths to versioned unity asset package repositories.")]
    public UTString VersionedPackagesPath;

    [UTInspectorHint(required = true, order = 2)]
    [UTDoc(description = "Paths to unity development projects.")]
    public UTString DevProjectsPath;


    public override IEnumerator Execute(UTContext context)
    {
        var config = BuildSystemConfigurationAssetUtil.CheckGet();

        config.configuration.OutputPath = OutputPath.EvaluateIn(context);
        config.configuration.DependencyProjectPaths = VersionedPackagesPath.EvaluateIn(context).Split(System.IO.Path.PathSeparator);
        config.configuration.DevProjectPaths = DevProjectsPath.EvaluateIn(context).Split(System.IO.Path.PathSeparator);

        yield break;
    }


    [MenuItem("Assets/Create/uTomate/FIE Build/SetBuildSystemConfiguration", false, 250)]
    public static void AddAction()
    {
        Create<SetBuildSystemConfiguration>();
    }

}
