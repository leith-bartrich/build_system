using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class VersionedPackagesUtil 
{
	public static Dictionary<string,List<VersionedPackage>> FindAllAvailableVersions(string[] paths){
		var ret = new Dictionary<string,List<VersionedPackage>> ();
		foreach (var path in paths) {
			var pathInfo = new DirectoryInfo (path);
			if (pathInfo.Exists) {
				foreach (var packagePath in pathInfo.GetDirectories()) {
					ret [packagePath.Name] = new List<VersionedPackage> ();
					foreach (var assetFile in packagePath.GetFiles(packagePath.Name + ".*.unitypackage")) {
						var tokens = assetFile.Name.Split (new char[]{'.'}, System.StringSplitOptions.RemoveEmptyEntries);
						var versionString = string.Join (".", tokens.Skip (1).Take (tokens.Length - 2).ToArray ());
						int[] version;
						if (VersionStringUtil.Parse (versionString, out version)) {
							ret [packagePath.Name].Add (new VersionedPackage (){ Name = packagePath.Name, Version = version, packageFile = assetFile});
						}
					}
				}
			}
		}
		return ret;
	}

	public static string ExportPathForAsset(ArtifactSettings settings, string outputPath){
		var artifactPath = Path.Combine (outputPath, settings.name);
		var artifactVersionPath = Path.Combine (artifactPath, System.String.Join(".",new string[]{settings.name,settings.version,"unitypackage"}));
		return artifactVersionPath;
	}


}



public class VersionedPackage{
	public string Name;
	public int[] Version;
	public FileInfo packageFile;
}