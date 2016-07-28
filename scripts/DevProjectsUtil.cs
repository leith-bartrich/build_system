using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DevProjectsUtil
{

	public static Dictionary<string,DirectoryInfo> GetAllDevProjects(string[] paths){
		var ret = new Dictionary<string,DirectoryInfo>();
		foreach (var path in paths) {
			var pathInfo = new DirectoryInfo (path);
			if (pathInfo.Exists) {
				foreach (var projectPath in pathInfo.GetDirectories()) {
					ret[projectPath.Name] = projectPath;
				}
			}
		}
		return ret;
	}

}

