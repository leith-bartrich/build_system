using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class ScriptableObjectUtility {
	
	/// <summary>
	/// Create new asset from <see cref="ScriptableObject"/> type with unique name at
	/// selected folder in project window. Asset creation can be cancelled by pressing
	/// escape key when asset is initially being named.
	/// </summary>
	/// <typeparam name="T">Type of scriptable object.</typeparam>
	public static T CreateAsset<T>() where T : ScriptableObject {
		var asset = ScriptableObject.CreateInstance<T>();
		ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
        return asset;
	}

    public static void MoveAsset(ScriptableObject o, string path)
    {
        if (!AssetDatabase.IsValidFolder(path)){
            throw new System.IO.DirectoryNotFoundException(path);
        }
        AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(o), path);
    }

	public static T[] GetAll<T>() where T : ScriptableObject {
		var assetsPaths = AssetDatabase.FindAssets ("t:" + typeof(T).Name);
		List<T> ret = new List<T> ();
		foreach (var p in assetsPaths) {
			ret.Add (AssetDatabase.LoadAssetAtPath<T> (AssetDatabase.GUIDToAssetPath( p)));
		}
		return ret.ToArray ();
	}

	public static bool Get<T>(out T found) where T : ScriptableObject {
		var all = GetAll<T> ();
		if (all.Length > 0) {
			found = all[0];
			return true;
		} else {
			found = null;
			return false;
		}
	}


}