using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class VersionStringUtil
{
	public VersionStringUtil ()
	{
	}

	public static bool Parse(string version, out int[] parsed){
		var tokens = version.Split (new string[]{"."}, StringSplitOptions.RemoveEmptyEntries);
		parsed = new int[tokens.Length];
		for (int i = 0; i < tokens.Length; i++) {
			int p;
			if (int.TryParse(tokens[i], out p)){
				parsed[i] = p;
			} else {
				parsed[i] = -1;
			}
		}
		foreach (int i in parsed) {
			if (i < 0){
				return false;
			}
		}
		return true;
	}

	public static string ToString(int[] version){
		var tokens = from t in version select t.ToString ();
		return string.Join (".", tokens.ToArray ());
	}

	public static int[] PadToLength(int[] version, int length){
		var retLength = Math.Max(length, version.Length);
		int[] ret = new int[retLength];
		for (int i = 0; i < retLength; i++) {
			if (i < version.Length){
				ret[i] = version[i];
			} else {
				ret[i] = 0;
			}
		}
		return ret;
	}

	public static int[] Max(int[] left, int[] right){
		var l = PadToLength (left, right.Length);
		var r = PadToLength (right, left.Length);

		for (int i = 0; i < l.Length; i++) {
			if (l[i] > r[i]){
				return left.Clone() as int[];
			} else if (l[i] < r[i]) {
				return right.Clone() as int[];
			}
		}

		if (left.Length > right.Length) {
			return left.Clone () as int[];
		} else {
			return right.Clone () as int[];
		}
	}



	public static bool Equivalent(int[] left, int[] right){
		var l = PadToLength (left, right.Length);
		var r = PadToLength (right, left.Length);

		for (int i = 0; i < l.Length; i++) {
			if (l[i] != r[i]){
				return false;
			}
		}
		return true;
	}


}

public class VersionSorter: IComparer<int[]> {

	#region IComparer implementation

	public int Compare (int[] left, int[] right)
	{
		var l = VersionStringUtil.PadToLength (left, right.Length);
		var r = VersionStringUtil.PadToLength (right, left.Length);
		
		for (int i = 0; i < l.Length; i++) {
			if (l[i] > r[i]){
				return -1;
			} else if (l[i] < r[i]) {
				return 1;
			}
		}
		if (left.Length > right.Length) {
			return -1;
		} else {
			return 1;
		}
		return -0;
	}

	#endregion


}

