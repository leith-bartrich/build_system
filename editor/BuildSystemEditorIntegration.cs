using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using AncientLightStudios.uTomate;
using AncientLightStudios.uTomate.API;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;


public static class BuildSystemEditorIntegration {

	//Settings

	[MenuItem("FIEBuild/Settings/Create Asset/Dependency")]
	public static void CreateDependencyAsset() {
		ScriptableObjectUtility.CreateAsset<UnityPackageDependencySettingsAsset> ();
	}

	[MenuItem("FIEBuild/Settings/Create Asset/Artifact")]
	public static void CreateArtifactAsset() {
		ScriptableObjectUtility.CreateAsset<ArtifactSettingsAsset> ();
	}

	[MenuItem("FIEBuild/Settings/Create Asset/Build System")]
	public static void CreateBuidSystemSettingsAsset() {
		ScriptableObjectUtility.CreateAsset<BuildSystemSettingsAsset> ();
	}

	//Configuration

	[MenuItem("FIEBuild/Configuration/Create Asset/Build System")]
	public static void CreateBuildSystemConfiguratinAsset() {
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
	}

	//System

	[MenuItem("FIEBuild/System/Check Dependencies")]
	public static void EvalDependencies(){
		Dictionary<string,UnityPackageDependencySettings> folded;
		string[] errors;
		bool foldSucceeded = UnityPackageDependencySettingAssetUtil.GetAllFolded (out folded, out errors);
		if (foldSucceeded) {
			UnityEngine.Debug.Log("Folded all dependencies.");
			foreach (var d in folded){
				UnityEngine.Debug.Log(d.Value.ToString()); 
			}
		} else {
			UnityEngine.Debug.LogWarning("Failed to fold all dependencies");
			foreach (var e in errors){
				UnityEngine.Debug.LogError(e);
			}
			return;
		}
		BuildSystemSettingsAsset buildSystemSettingsAsset;
		if (!BuildSystemSettingsAssetUtil.Get(out buildSystemSettingsAsset)){
			UnityEngine.Debug.LogError("No Build System Settings Asset in this project.");
			return;
		}
	}

	[MenuItem("FIEBuild/System/Import Selected Dependencies")]
	public static void ImportSelectedDependencies(){
		var guids = Selection.assetGUIDs;
		var assets = new List<UnityPackageDependencySettingsAsset> ();
		foreach (var guid in guids) {
			var path = AssetDatabase.GUIDToAssetPath (guid);
			var asset = AssetDatabase.LoadAssetAtPath<UnityPackageDependencySettingsAsset> (path);
			if (asset != null) {
				assets.Add (asset);
			}
		}
		ImportPackages (assets.ToArray (),false);
	}

	[MenuItem("FIEBuild/System/Import All Dependencies")]
	public static void ImportAllDependencies(){
		var assets = UnityPackageDependencySettingAssetUtil.GetAll ();

		var didImport = ImportPackages (assets.ToArray (),true);
		if (didImport) {
			ImportAllDependencies();
		}
	}

	public static bool ImportPackages(UnityPackageDependencySettingsAsset[] assets, bool checkAlreadyImported){
		Dictionary<string,UnityPackageDependencySettings> folded;
		string[] errors;
		bool foldSucceeded = UnityPackageDependencySettingAssetUtil.GetAllFolded (out folded, out errors);
		if (!foldSucceeded) {
			UnityEngine.Debug.LogError("Failed to fold all dependencies");
			foreach (var e in errors){
				UnityEngine.Debug.LogError(e);
			}
			return false;
		}

		bool didImport = false;

		//RELEASE
		var versionedAssets = (from a in assets where a.dependency.importMode == UnityPackageImportMode.RELEASE select a).ToArray ();
		foreach (var asset in versionedAssets) {
			var settings = folded[asset.name];
			VersionedPackage versionedPackaged;
			if (! FindVersionedPackage(asset.dependency, out versionedPackaged)){
				UnityEngine.Debug.LogError("Could not satsify dependency: " + asset.dependency.name, asset);
				continue;
			}
			didImport = didImport | ImportVersionedPackage(versionedPackaged, checkAlreadyImported);
		}

		//DEV
		var devAssets = (from a in assets where a.dependency.importMode == UnityPackageImportMode.DEVPROJECT select a).ToArray ();
		foreach (var asset in devAssets) {
			throw new System.NotImplementedException();
			//TODO: do dev import
		}

		//return
		return didImport;
	}

	public static bool FindVersionedPackage(UnityPackageDependencySettings settings, out VersionedPackage found){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		var availableVersions = VersionedPackagesUtil.FindAllAvailableVersions (config.configuration.DependencyProjectPaths);
		switch (settings.importMode) {
		case UnityPackageImportMode.RELEASE:
			if (!availableVersions.ContainsKey(settings.name)){
				UnityEngine.Debug.LogWarning("Package " + settings.name + " was not found in the search paths.");
				found = null;
				return false;
			}
			var versions = availableVersions[settings.name];
			var satisfiedVersions = from v in versions where settings.Satisfied(v) select v;
			var highest = satisfiedVersions.OrderBy(o=>o.Version,new VersionSorter()).Last();
			found = highest;
			return true;
		default:
			UnityEngine.Debug.LogError("Cannot ask for a versioned package for this import type.");
			found = null;
			return false;
		}
	}

	public static bool ImportDevProjectPackage(string projectPath, string artifactName){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		var tempPath = System.IO.Path.GetTempPath ();
		var outputDir = new System.IO.DirectoryInfo (System.IO.Path.Combine (tempPath, System.Guid.NewGuid().ToString()));
		System.IO.Directory.CreateDirectory (outputDir.FullName);
		//TODO: build project

		//configure
		var configureProperties = new Dictionary<string,string> ();
		configureProperties ["OutputPath"] = outputDir.FullName;
		configureProperties ["VersionedPackagesPath"] = System.String.Join (Path.PathSeparator.ToString (), config.configuration.DependencyProjectPaths);
		configureProperties ["DevProjectsPath"] = System.String.Join (Path.PathSeparator.ToString (), config.configuration.DevProjectPaths);
		if (!RunUTExternalPlan (projectPath, "configure", false, configureProperties)) {
			UnityEngine.Debug.LogError("Configure failed for project: " + projectPath);
			throw new System.Exception();
		}

		//make
		if (!RunUTExternalPlan (projectPath, artifactName + "_make", false, new Dictionary<string, string> ())) {
			UnityEngine.Debug.LogError("Make failed for " + artifactName + " in project: " + projectPath);
			throw new System.Exception();
		}

		//export
		if (!RunUTExternalPlan (projectPath, artifactName + "_export", false, new Dictionary<string, string>())) {
			UnityEngine.Debug.LogError("Export failed for " + artifactName + " in project: " + projectPath);
			throw new System.Exception();
		}


		var packageFile = outputDir.GetFiles ("*.unitypackage").FirstOrDefault();
		if (packageFile == null) {
			UnityEngine.Debug.LogError("Build DEV package not found.");
			return false;
		}
		AssetDatabase.ImportPackage (packageFile.FullName, false);
		Directory.Delete (outputDir.FullName, true);

		config.configuration.importedPackages = config.configuration.importedPackages.Concat(new BuildSystemConfiguration.ImportedPackage[]{
			new BuildSystemConfiguration.ImportedPackage(){
				name = artifactName, version = System.IO.Path.GetFileName(projectPath)
			}
		}).ToArray();

		return true;
	}

		private static bool CancelUTExternalPlan = false;

	public static bool RunUTExternalPlan(string theProjectPath, string thePlanName, bool theDebugMode, Dictionary<string,string> properties){
		if (!Directory.Exists(theProjectPath))
		{
			throw new System.Exception("Project path " + theProjectPath + " does not exist.");
		}
		
		if (UTFileUtils.IsBelow(UTFileUtils.ProjectRoot, theProjectPath))
		{
			throw new System.Exception("You cannot run uTomate externally on the current project. Use the Sub-Plan node if you want to run a plan as part of a plan.");
		}

		StringBuilder sb = new StringBuilder();

		var theProperties = (from p in properties select p.Key + "=" + p.Value).ToArray ();

		foreach (var prop in theProperties)
		{
			sb.Append(" -prop ").Append(UTExecutableParam.Quote(prop));
		}
		
		var theUnityEditorExecutable = UTils.GetEditorExecutable();
		theUnityEditorExecutable = UTils.CompleteEditorExecutable(theUnityEditorExecutable);


		Process process = new Process();
		process.StartInfo.FileName = theUnityEditorExecutable;
		process.StartInfo.Arguments = "-projectPath " + UTExecutableParam.Quote(theProjectPath) +
			" -executeMethod UTExternalRunner.RunPlan -plan " + UTExecutableParam.Quote(thePlanName) +
				" -debugMode " + theDebugMode + sb.ToString();

		try
		{
			if (!process.Start())
			{
				throw new System.Exception("Unable to start Unity3D.");
			}
		}
		catch (Win32Exception e)
		{
			throw new System.Exception("Unable to start process " + e.Message);
		}

		do
		{
			System.Threading.Thread.Sleep(0);
			if (CancelUTExternalPlan && !process.HasExited)
			{
				process.Kill();
			}
		} while (!process.HasExited);
		
		if (!CancelUTExternalPlan)
		{
			if (process.ExitCode != 0)
			{
				throw new System.Exception("Plan " + thePlanName + " failed or was cancelled.");
			}
		}

		return true;
	}

	public static bool ImportVersionedPackage(VersionedPackage package, bool checkAlreadyImported){
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();

		if (checkAlreadyImported) {
			var existing = (from c in config.configuration.importedPackages where c.name == package.Name select c).FirstOrDefault ();
			if (existing != null){
				int[] version;
				if (VersionStringUtil.Parse(existing.version,out version)){
					if (VersionStringUtil.Equivalent(version,package.Version)){
						return false;
					}
				}
			}
		}

		AssetDatabase.ImportPackage (package.packageFile.FullName, false);
		config.configuration.importedPackages = config.configuration.importedPackages.Concat(new BuildSystemConfiguration.ImportedPackage[]{
			new BuildSystemConfiguration.ImportedPackage(){
				name = package.Name, version = VersionStringUtil.ToString(package.Version)
			}
		}).ToArray();
		return true;
	}

	[MenuItem("FIEBuild/System/Clean All")]
	public static void CleanAll(){
		UnityEngine.Debug.Log ("Cleaning all imported packages");
		var config = BuildSystemConfigurationAssetUtil.CheckGet ();
		foreach (var package in config.configuration.importedPackages) {
			UnityEngine.Debug.Log("Cleaning Package: " + package.name + " " + package.version);
			foreach (var delPath in package.cleanPaths){
				UnityEngine.Debug.Log("Deleting: " + delPath);
				AssetDatabase.DeleteAsset(delPath);
			}
		}
		config.configuration.importedPackages = new BuildSystemConfiguration.ImportedPackage[0];
		UnityEngine.Debug.Log ("Done cleaning all imported packages.");
	}



}
