using UnityEngine;
using JMTKGUI;
using GUIElement = JMTKGUI.GUIElement; // Use JMTKGUI.GUIElement instead of 
// A hacky showcase of the current GUI system included in the Modding Toolkit
class PhysicsManipulation
{
    // A couple of TextGUIElement instances which can represent 
        // TextField : A one line string that the user can enter
        // TextArea : A multiline string that the user can enter
        // Label : An uneditable string that will be displayed on screen
    // Constructor for TextGUIElement:
        // Rect : A rect that, in order to be scalable over the screen is expressed in terms of fractions of screen width and height
        // string : The string to put into the TextGUI element
        // JMTKGUI.GUIElement : The type of GUI element, this is an enum to specify how to render a Custom GUIElement, for text elements TextField,Label,TextArea
    public static TextGUIElement xElem = new TextGUIElement(new Rect(0.85f,0.955f,0.05f,0.025f),"0",GUIElement.TextField);
	public static TextGUIElement yElem = new TextGUIElement(new Rect(0.90f,0.955f,0.05f,0.025f),Physics.gravity.y+"",GUIElement.TextField);
	public static TextGUIElement zElem = new TextGUIElement(new Rect(0.95f,0.955f,0.05f,0.025f),"0",GUIElement.TextField);
	public static TextGUIElement timeScaleElem = new TextGUIElement(new Rect(0.95f,0.925f,0.05f,0.025f),Time.timeScale+"",GUIElement.TextField);
    public static float x = 0f;
    public static float y = 0f;
    public static float z = 0f;
    public static float timeScale = 1f;
    // Code to run on Start
    static void Start()
	{
        // Add all the important guiElements, they will only show up on screen when added here!
        UMods.JMTK.instance.guiElements.Add(xElem);
        UMods.JMTK.instance.guiElements.Add(yElem);
        UMods.JMTK.instance.guiElements.Add(zElem);
        UMods.JMTK.instance.guiElements.Add(timeScaleElem);
	}
    // Code to run on Update, every ingame frame!
    static void Update()
	{
        // Try to parse floats from strings the user enters, if it doesnt work, dont use it.
        bool xParseSuccess = float.TryParse(xElem.content,out x);
        bool yParseSuccess = float.TryParse(yElem.content,out y);
        bool zParseSuccess = float.TryParse(zElem.content,out z);
        bool timeScaleParseSuccess = float.TryParse(timeScaleElem.content,out timeScale);
        // Set gravity and timeScale to the parsed values
        Physics.gravity = new Vector3(x,y,z);
        Time.timeScale = timeScale;
	}
    // Code to run when this mod is removed from the modlist. Clean up your trash here!
    static void Remove()
	{
        // Remove elements from the guiElements list, because they are not needed anymore.
        UMods.JMTK.instance.guiElements.Remove(xElem);
        UMods.JMTK.instance.guiElements.Remove(yElem);
        UMods.JMTK.instance.guiElements.Remove(zElem);
        UMods.JMTK.instance.guiElements.Remove(timeScaleElem);
	}
}