using UnityEngine;
using System.Collections;
using AncientLightStudios.uTomate;
using AncientLightStudios.uTomate.API;
using System.Linq;

[UTScriptExtension("fiebuild")]
public class BuildSystemUTExtension
{
	public string PackageArtifactVersion(string name){
		var artifact = (from a in ArtifactSettingsAssetUtil.GetAll () where
			((a.artifact.buildMethod == ArtifactBuildMethod.UNITYPACKAGE) & (a.artifact.name == name))
		 select a.artifact
		 ).FirstOrDefault ();
		if (artifact == null) {
			return "";
		}
		return artifact.version;
	}

	public string OutputPath(){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		return config.configuration.OutputPath;
	}

	public string OutputPackageDirPath(string name){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		var artifact = (from a in ArtifactSettingsAssetUtil.GetAll () where
		                ((a.artifact.buildMethod == ArtifactBuildMethod.UNITYPACKAGE) & (a.artifact.name == name))
		                select a.artifact
		                ).FirstOrDefault ();
		if (artifact == null) {
			throw new UTFailBuildException("No artifact found for given name: " + name,null);
		}
		return System.IO.Path.Combine (config.configuration.OutputPath, name);
	}


	public string OutputPackageFilePath(string name){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		var artifact = (from a in ArtifactSettingsAssetUtil.GetAll () where
		                ((a.artifact.buildMethod == ArtifactBuildMethod.UNITYPACKAGE) & (a.artifact.name == name))
		                select a.artifact
		                ).FirstOrDefault ();
		if (artifact == null) {
			throw new UTFailBuildException("No artifact found for given name: " + name,null);
		}
		return VersionedPackagesUtil.ExportPathForAsset(artifact,config.configuration.OutputPath);
	}

	public BuildSystemConfiguration Configuration(){
		return BuildSystemConfigurationAssetUtil.CheckGet().configuration;
	}

}

