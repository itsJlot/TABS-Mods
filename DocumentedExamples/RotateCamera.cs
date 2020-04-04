// Super basic example Mod, camera will rotate by 1 degree each frame.

// Using statements to import namespaces
using UnityEngine;
using Landfall.TABS;
// Any class name works here! Modding Toolkit will search out all methods in the file
class RotateCamera
{
	// Static methods only, no Object oriented Programming working yet!
	// Modding toolkit searches for 3 method names:
	// 		Update : Callback each frame
	// 		Remove : Callback when this mod is removed. Cleanup things you created here!
	//		OnGUI : Callback each time GUI is rendered, pretty much useless
	//		All methods with different names will only be called once at startup!
	static void Update()
	{
		// Rotate camera using Rotate Method
        Camera.main.transform.Rotate(new Vector3(0,1,0));
	}
}