using UnityEngine;
using System.Collections;

public static class ArtifactSettingsAssetUtil {
	
	public static ArtifactSettingsAsset[] GetAll(){
		return ScriptableObjectUtility.GetAll<ArtifactSettingsAsset> ();
	}

}
