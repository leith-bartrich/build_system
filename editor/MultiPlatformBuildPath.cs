using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[System.Serializable]
public class MultiPlatformBuildPath{
	public string WindowsBase;
	public string OSXBase;
	public string LinuxBase;
	public string Appended;

	public string GetPath(){
#if UNITY_EDITOR_WIN
		return Path.Combine(this.WindowsBase,this.Appended);
#endif
#if UNITY_EDITOR_OSX
		return Path.Combine(this.OSXBase,this.Appended);
#endif
#if UNITY_EDITOR_LINUX
		return Path.Combine(this.LinuxBase,this.Appended);
#endif
	}
}
