using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class UnityPackageDependencySettingAssetUtil {
	
	public static UnityPackageDependencySettingsAsset[] GetAll(){
		return ScriptableObjectUtility.GetAll<UnityPackageDependencySettingsAsset> ();
	}

	public static bool GetAllFolded( out Dictionary<string,UnityPackageDependencySettings> folded, out string[] errors){
		var collected = new Dictionary<string,List<UnityPackageDependencySettings>> ();
		foreach (var d in GetAll()){
			if (!collected.ContainsKey(d.dependency.name)){
				collected[d.dependency.name] = new List<UnityPackageDependencySettings>();
			}
			collected[d.dependency.name].Add(d.dependency);
		}

		folded = new Dictionary<string,UnityPackageDependencySettings> ();

		var errorsList = new List<string> ();

		foreach (var k in collected.Keys) {
			UnityPackageDependencySettings d = collected[k].First();
			for (int i = 1; i < collected[k].Count; i++){
				UnityPackageDependencySettings f;
				string reason;
				if (d.Fold(collected[k][i], out f, out reason)){
					d = f;
				} else {
					errorsList.Add("Error folding " + k + ": " + reason);
				}
			}
			folded[k] = d;
		}

		errors = errorsList.ToArray ();
		return errors.Length == 0;
	}
}
