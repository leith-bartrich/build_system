using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class BuildSystemConfigurationAssetUtil {

	public static BuildSystemConfigurationAsset[] GetAll(this BuildSystemConfigurationAsset self){
		return ScriptableObjectUtility.GetAll<BuildSystemConfigurationAsset> ();
	}

	public static bool Get( out BuildSystemConfigurationAsset found){
		return ScriptableObjectUtility.Get<BuildSystemConfigurationAsset>(out found);
	}

	public static BuildSystemConfigurationAsset CheckGet(){
		BuildSystemConfigurationAsset ret;
		if (Get( out ret)){
			return ret;
		} else {

			ret = ScriptableObjectUtility.CreateAsset<BuildSystemConfigurationAsset> ();
			//ScriptableObjectUtility.MoveAsset(ret, "build_configuration");
			return ret;
		}
	}

}
