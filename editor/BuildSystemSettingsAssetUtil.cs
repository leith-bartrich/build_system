using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BuildSystemSettingsAssetUtil {

	public static BuildSystemSettingsAsset[] GetAll(this BuildSystemSettingsAsset self){
		return ScriptableObjectUtility.GetAll<BuildSystemSettingsAsset> ();
	}

	public static bool Get(out BuildSystemSettingsAsset found){
		return ScriptableObjectUtility.Get<BuildSystemSettingsAsset>(out found);
	}

}
