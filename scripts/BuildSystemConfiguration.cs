using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class BuildSystemConfiguration 
{
	public string[] DependencyProjectPaths = new string[0];
	public string[] DevProjectPaths = new string[0];
	public string OutputPath;
	public ImportedPackage[] importedPackages = new ImportedPackage[0];

	[System.Serializable]
	public class ImportedPackage{
		public string name;
		public string version;
		public string[] cleanPaths = new string[0]; 
	}
}

