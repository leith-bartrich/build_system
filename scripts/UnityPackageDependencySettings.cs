using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;


[System.Serializable]
public class UnityPackageDependencySettings {

	public string name;
	public UnityPackageImportMode importMode;
	public string requirement;
	public VersionMatchType versionMatchType;


	public override string ToString ()
	{
		var modeString = System.Enum.GetName(typeof(UnityPackageImportMode), this.importMode);
		return string.Format ("[Dependency " + this.name + " " + modeString + " " + requirement + "]");
	}

	public bool Satisfied(VersionedPackage versioned){
		int[] version;
		if (!VersionStringUtil.Parse (requirement, out version)) {
			Debug.LogWarning("Dependency settings requirement isn't a version string.");
			return false;
		}
		if (versionMatchType == VersionMatchType.EXACT) {
			return VersionStringUtil.Equivalent(version,versioned.Version);
		}
		if (versionMatchType == VersionMatchType.HIGHEST) {
			return true;
		}
		if (versionMatchType == VersionMatchType.SEMANTIC) {
			version = VersionStringUtil.PadToLength(version,3);
			var other = VersionStringUtil.PadToLength(versioned.Version,3);
			if (version.Length > 3 || other.Length > 3){
				Debug.LogWarning("Version numbers are not semantic. Too long.");
				return false;
			}
			if (version[0] != other[0]){
				return false;
			}
			if (other[1] < version[1]){
				return false;
			}
			if (other[1] == version[1]){
				if (other[2] < version[2]){
					return false;
				}
			}
			return true;
		}
		throw new System.NotImplementedException ("Unknown version match type.");
	}


	public bool Fold(UnityPackageDependencySettings other, out UnityPackageDependencySettings folded, out string reason){

		if (other.importMode != this.importMode){
			reason = "Different Modes.";
			folded = null;
			return false;
		}

		if (other.versionMatchType != this.versionMatchType) {
			reason = "Different version match types.";
			folded = null;
			return false;
		}

		if (this.importMode == UnityPackageImportMode.DEVPROJECT) {
			if (this.requirement == other.requirement){
				folded = new UnityPackageDependencySettings(){ name = this.name, importMode = this.importMode, requirement = this.requirement};
				reason = "OK";
				return true;			
			} else {
				folded = null;
				reason = "Requirements do not match.";
				return false;
			}
		}


		int[] versionThis;
		if (!(VersionStringUtil.Parse(this.requirement,out versionThis))){
			reason = "Could not parse Version: " + this.requirement;
			folded = null;
			return false;
		}

		int[] versionOther;
		if (!(VersionStringUtil.Parse(other.requirement,out versionOther))){
			reason = "Could not parse Version: " + other.requirement;
			folded = null;
			return false;
		}

		if (this.versionMatchType == VersionMatchType.EXACT) {
			if (!VersionStringUtil.Equivalent(versionThis,versionOther)){
				reason = "Match type is EXACT but versions are not the same.";
				folded = null;
				return false;
			}
		}

		if (this.versionMatchType == VersionMatchType.SEMANTIC) {
			if (versionThis[0] != versionOther[0]){
				reason = "Match type is SEMANTIC but the Major version number is not the same.";
				folded = null;
				return false;
			}
		}

		folded = new UnityPackageDependencySettings(){ name = this.name, importMode = this.importMode, requirement = VersionStringUtil.ToString(VersionStringUtil.Max(versionThis,versionOther))};
		reason = "OK";
		return true;
	}

}
