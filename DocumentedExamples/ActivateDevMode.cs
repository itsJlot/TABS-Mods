using UnityEngine;
// Simple example mod, sets a variable and resets it when removed
class DevMode
{
	// The start method will be called when the mod is installed
	static void Start()
	{
        Debug.Log("Start called!");
		// Activate dev mode in the Modding toolkit
		UMods.JMTK.instance.devMode = true;
	}
	// The Remove method will be called when the mod is installed
    static void Remove()
	{
        Debug.Log("Remove called!");
		// Turn dev mode back off when uninstalled
		UMods.JMTK.instance.devMode = false;
	}
}