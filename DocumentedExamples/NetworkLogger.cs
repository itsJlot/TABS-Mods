using UnityEngine;
class NetworkLogger
{
	static void Start()
	{
        Debug.Log("Start called!");
		UMods.JMTK.instance.useNetworkLogger = true;
	}
    static void Remove()
	{
        Debug.Log("Remove called!");
		UMods.JMTK.instance.useNetworkLogger = false;
	}
}
