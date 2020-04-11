using System;
using System.Collections.Generic;
using System.Reflection;
using JMTKGUI;
using UMods;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class Inspector
{
	public static void Start()
	{
		for (int i = 0; i < Inspector.gameObjects.Length; i++)
		{
			Inspector.gameObjects[i] = new List<CustomGUI>();
		}
		Inspector.CreateList(null);
		Button button4 = new Button(new Rect(0f, 0.18f, 0.1f, 0.02f), "Refresh List", JMTKGUI.GUIElement.Button, delegate()
		{
			try
			{
				Inspector.CreateList(null);
			}
			catch (Exception)
			{
				Debug.Log("There is an issue");
			}
		});
		JMTK.instance.guiElements.Add(button4);
	}

	public static void CreateList(GameObject parentObject = null)
	{
		try
		{
			int layer = 0;
			if (parentObject != null)
			{
				layer = Inspector.CountParents(parentObject);
				try
				{
					Inspector.CreateComponentList(parentObject);
				}
				catch (Exception)
				{
					Debug.Log("Concurrent modification while listing thing?");
				}
			}
			for (int i = layer; i < Inspector.gameObjects.Length; i++)
			{
				Inspector.RemoveAllFromGUI(Inspector.gameObjects[i]);
				Inspector.gameObjects[i] = new List<CustomGUI>();
			}
			float j = 0.2f;
			List<GameObject> transforms = new List<GameObject>();
			if (!parentObject)
			{
				foreach (Scene scene in SceneManager.GetAllScenes())
				{
					transforms.AddRange(new List<GameObject>(scene.GetRootGameObjects()));
				}
			}
			else
			{
				transforms = new List<GameObject>();
				foreach (object obj in parentObject.transform)
				{
					Transform transform = (Transform)obj;
					transforms.Add(transform.gameObject);
				}
			}
			using (List<GameObject>.Enumerator enumerator2 = transforms.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					GameObject gameObject = enumerator2.Current;
					Button button4 = new Button(new Rect(0.1f * (float)layer, j, 0.1f, 0.02f), gameObject.name, JMTKGUI.GUIElement.Button, delegate()
					{
						try
						{
							Inspector.CreateList(gameObject);
						}
						catch (Exception e2)
						{
							Debug.Log(e2 + "\n" + e2.StackTrace);
						}
					});
					j += 0.02f;
					Inspector.gameObjects[layer].Add(button4);
				}
			}
			Inspector.AddAllToGUI(Inspector.gameObjects[layer]);
		}
		catch (Exception e)
		{
			Debug.Log(e + "\n" + e.StackTrace);
		}
	}

	private static void Update()
	{
	}

	private static void Remove()
	{
	}

	public Inspector()
	{
	}

	static Inspector()
	{
		Inspector.componentChoice = new List<CustomGUI>();
		Inspector.gameObjects = new List<CustomGUI>[8];
		Inspector.components = new List<CustomGUI>();
		Inspector.maxDepth = 8;
	}

	public static int CountParents(GameObject go)
	{
		if (go.transform.parent)
		{
			return 1 + Inspector.CountParents(go.transform.parent.gameObject);
		}
		return 1;
	}

	public static void CreateComponentList(GameObject go)
	{
		Inspector.RemoveAllFromGUI(Inspector.componentChoice);
		Inspector.InspectComponent(null);
		Inspector.componentChoice = new List<CustomGUI>();
		Component[] array = go.GetComponents<Component>();
		for (int i = 0; i < array.Length; i++)
		{
			Component component = array[i];
			Tuple<Rect, Rect> tuple = Inspector.SplitRect(new Rect(0.7f, 0.4f + 0.02f * (float)i, 0.1f, 0.02f), 0.1f);
			Button destroyB = new Button(tuple.Item1, "X", JMTKGUI.GUIElement.Button, delegate()
			{
				if (component.GetType() == typeof(Transform))
				{
					UnityEngine.Object.Destroy(((Transform)component).gameObject);
					Inspector.CreateComponentList(go);
					return;
				}
				UnityEngine.Object.Destroy(component);
				Inspector.CreateComponentList(go);
			});
			Button b = new Button(tuple.Item2, component.GetType().Name, JMTKGUI.GUIElement.Button, delegate()
			{
				Inspector.InspectComponent(component);
			});
			Inspector.componentChoice.Add(b);
			Inspector.componentChoice.Add(destroyB);
		}
		Inspector.AddAllToGUI(Inspector.componentChoice);
	}

	public static void InspectComponent(Component component)
	{
		float startX = 0.8f;
		Inspector.RemoveAllFromGUI(Inspector.componentFields);
		if (!component)
		{
			return;
		}
		Inspector.componentFields.Add(new TextGUIElement(new Rect(startX, 0.4f - Inspector.unitHeight, 0.2f, Inspector.unitHeight), component.name + " Fields", JMTKGUI.GUIElement.Label, null));
		Inspector.componentFields = new List<CustomGUI>();
		int i = 0;
		foreach (PropertyInfo field in component.GetType().GetProperties())
		{
			try
			{
				Tuple<List<CustomGUI>, int> tuple = Inspector.CreateEditField(component, field, new Rect(0.8f, 0.4f + Inspector.unitHeight * (float)i, 0.2f, Inspector.unitHeight));
				List<CustomGUI> editFields = tuple.Item1;
				int height = tuple.Item2;
				i += height;
				if (editFields != null)
				{
					Inspector.AddAllToGUI(editFields);
					Inspector.componentFields.AddRange(editFields);
				}
				else
				{
					Debug.Log(string.Concat(new object[]
					{
						"Key: ",
						field.Name,
						" Value: ",
						field.GetValue(component)
					}));
				}
			}
			catch (Exception e)
			{
				Debug.Log("Issue with " + field.Name);
				Debug.Log(string.Concat(new object[]
				{
					"Inspector problem with field: " + field.Name + " \nException: ",
					e,
					"\n",
					e.StackTrace
				}));
			}
		}
	}

	public static void RemoveAllFromGUI(List<CustomGUI> elements)
	{
		JMTK.instance.guiRemovalQueue.AddRange(elements);
	}

	public static void AddAllToGUI(List<CustomGUI> elements)
	{
		JMTK.instance.guiAddQueue.AddRange(elements);
	}

	public static Tuple<Rect, Rect> SplitRect(Rect r, float ratio)
	{
		Rect r2 = new Rect(r);
		r2.xMax = Mathf.Lerp(r2.xMin, r2.xMax, ratio);
		Rect r3 = new Rect(r);
		r3.xMin = r2.xMax;
		return new Tuple<Rect, Rect>(r2, r3);
	}

	public static Tuple<List<CustomGUI>, int> CreateEditField(object o, PropertyInfo field, Rect space)
	{
		int height = 0;
		Type typeOfFieldContent = field.PropertyType;
		Debug.Log("This type:" + typeOfFieldContent.Name + "\t | Base: " + typeOfFieldContent.BaseType.Name);
		List<CustomGUI> i = new List<CustomGUI>();
		Tuple<Rect, Rect> r = Inspector.SplitRect(space, 0.7f);
		if (typeOfFieldContent.Name == "UInt16")
		{
			TextGUIElement intInput = new TextGUIElement(r.Item2, field.GetValue(o).ToString(), JMTKGUI.GUIElement.TextField, delegate()
			{
			});
			i.Add(intInput);
			i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
			{
				field.SetValue(o, int.Parse(intInput.content));
			}));
			height = 1;
		}
		else if (typeOfFieldContent.Name == "Int32" || typeOfFieldContent.Name == "UInt32")
		{
			TextGUIElement intInput = new TextGUIElement(r.Item2, field.GetValue(o).ToString(), JMTKGUI.GUIElement.TextField, delegate()
			{
			});
			i.Add(intInput);
			i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
			{
				field.SetValue(o, long.Parse(intInput.content));
			}));
			height = 1;
		}
		else
		{
			if (typeOfFieldContent.Name == "String")
			{
				height = 1;
				try
				{
					TextGUIElement boolIn = new TextGUIElement(r.Item2, field.GetValue(o).ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					i.Add(boolIn);
					i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						field.SetValue(o, boolIn.content);
					}));
					goto IL_F6E;
				}
				catch (Exception ex)
				{
					Debug.Log(ex.StackTrace);
					Debug.Log("This is not how booleans work. :(");
					goto IL_F6E;
				}
			}
			if (typeOfFieldContent.Name == "Rect")
			{
				r = Inspector.SplitRect(space, 1f);
				Rect firstRow = new Rect(r.Item1);
				Vector2 center = firstRow.center;
				center.y += Inspector.unitHeight;
				firstRow.center = center;
				Tuple<Rect, Rect> x_y_rect = Inspector.SplitRect(firstRow, 0.5f);
				Rect secondRow = new Rect(r.Item1);
				Vector2 center2 = secondRow.center;
				center2.y += Inspector.unitHeight * 2f;
				secondRow.center = center2;
				Tuple<Rect, Rect> tuple4 = Inspector.SplitRect(secondRow, 0.5f);
				Rect xRect = x_y_rect.Item1;
				Rect yRect = x_y_rect.Item2;
				Rect wRect = tuple4.Item1;
				Rect hRect = tuple4.Item2;
				Rect vector = (Rect)field.GetValue(o);
				TextGUIElement xInput = new TextGUIElement(xRect, vector.x.ToString(), JMTKGUI.GUIElement.TextField, delegate()
				{
				});
				TextGUIElement yInput = new TextGUIElement(yRect, vector.y.ToString(), JMTKGUI.GUIElement.TextField, delegate()
				{
				});
				TextGUIElement wInput = new TextGUIElement(wRect, vector.width.ToString(), JMTKGUI.GUIElement.TextField, delegate()
				{
				});
				TextGUIElement hInput = new TextGUIElement(hRect, vector.height.ToString(), JMTKGUI.GUIElement.TextField, delegate()
				{
				});
				i.Add(xInput);
				i.Add(yInput);
				i.Add(wInput);
				i.Add(hInput);
				i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
				{
					field.SetValue(o, new Rect(float.Parse(xInput.content), float.Parse(yInput.content), float.Parse(wInput.content), float.Parse(hInput.content)));
				}));
				height = 3;
			}
			else
			{
				if (typeOfFieldContent.Name == "Bounds")
				{
					height = 3;
					Bounds bounds = (Bounds)field.GetValue(o);
					try
					{
						Debug.Log("Got bounds: " + bounds);
						Rect row = new Rect(space);
						i.Add(new TextGUIElement(row, "BOUNDS: " + field.Name, JMTKGUI.GUIElement.Label, delegate()
						{
						}));
						Debug.Log("Got bounds: " + bounds);
						Rect row2 = new Rect(space);
						row2.Set(row2.x, row2.y + Inspector.unitHeight, row2.width, row2.height);
						Rect row3 = new Rect(space);
						row2.Set(row2.x, row2.y + Inspector.unitHeight * 2f, row2.width, row2.height);
						Debug.Log("Created rects");
						PropertyInfo pi = bounds.GetType().GetProperty("center");
						PropertyInfo ei = bounds.GetType().GetProperty("extents");
						Debug.Log("Property info: " + pi);
						Tuple<List<CustomGUI>, int> centerEdit = Inspector.CreateEditField(bounds.center, pi, row2);
						Tuple<List<CustomGUI>, int> extentsEdit = Inspector.CreateEditField(bounds.extents, ei, row3);
						i.AddRange(centerEdit.Item1);
						i.AddRange(extentsEdit.Item1);
					}
					catch (Exception e)
					{
						Debug.Log(string.Concat(new object[]
						{
							bounds,
							"\n",
							bounds,
							"n",
							"Bounds: ",
							e,
							"\n",
							e.StackTrace
						}));
					}
				}
				if (typeOfFieldContent.Name == "GameObject")
				{
					i.Add(new Button(space, string.Concat(new string[]
					{
						"GO: ",
						field.Name,
						"(",
						((GameObject)field.GetValue(o)).name,
						")"
					}), JMTKGUI.GUIElement.Button, delegate()
					{
						Inspector.CreateList((GameObject)field.GetValue(o));
						Inspector.InspectComponent(null);
					}));
					height = 1;
				}
				if (typeOfFieldContent.BaseType == typeof(Component))
				{
					i.Add(new Button(space, "Component: " + field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						Component component = (Component)field.GetValue(o);
						Inspector.CreateList(component.gameObject);
						Inspector.InspectComponent(component);
					}));
					height = 1;
				}
				if (typeOfFieldContent.Name == "Single")
				{
					TextGUIElement intInput = new TextGUIElement(r.Item2, field.GetValue(o).ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					i.Add(intInput);
					i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						field.SetValue(o, float.Parse(intInput.content));
					}));
					height = 1;
				}
				else if (typeOfFieldContent.Name == "Vector3")
				{
					r = Inspector.SplitRect(space, 0.4f);
					Tuple<Rect, Rect> tuple5 = Inspector.SplitRect(r.Item2, 0.33f);
					Tuple<Rect, Rect> y_zRect2 = Inspector.SplitRect(tuple5.Item2, 0.5f);
					Rect item = tuple5.Item1;
					Rect yRect2 = y_zRect2.Item1;
					Rect zRect2 = y_zRect2.Item2;
					Rect screenRect4 = item;
					Vector3 vector2 = (Vector3)field.GetValue(o);
					TextGUIElement xInput = new TextGUIElement(screenRect4, vector2.x.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					Rect screenRect5 = yRect2;
					vector2 = (Vector3)field.GetValue(o);
					TextGUIElement yInput = new TextGUIElement(screenRect5, vector2.y.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					Rect screenRect6 = zRect2;
					vector2 = (Vector3)field.GetValue(o);
					TextGUIElement zInput = new TextGUIElement(screenRect6, vector2.z.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					i.Add(xInput);
					i.Add(yInput);
					i.Add(zInput);
					i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						field.SetValue(o, new Vector3(float.Parse(xInput.content), float.Parse(yInput.content), float.Parse(zInput.content)));
					}));
					height = 1;
				}
				else if (typeOfFieldContent.Name == "Vector4")
				{
					r = Inspector.SplitRect(space, 0.4f);
					Tuple<Rect, Rect> tuple2 = Inspector.SplitRect(r.Item2, 0.25f);
					Tuple<Rect, Rect> tuple6 = Inspector.SplitRect(tuple2.Item2, 0.33f);
					Tuple<Rect, Rect> z_wRect3 = Inspector.SplitRect(tuple6.Item2, 0.5f);
					Rect xRect2 = tuple2.Item1;
					Rect yRect3 = tuple6.Item1;
					Rect zRect3 = z_wRect3.Item1;
					Rect wRect2 = z_wRect3.Item2;
					Vector4 vector3 = (Vector4)field.GetValue(o);
					TextGUIElement xInput = new TextGUIElement(xRect2, vector3.x.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement yInput = new TextGUIElement(yRect3, vector3.y.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement zInput = new TextGUIElement(zRect3, vector3.z.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement wInput = new TextGUIElement(wRect2, vector3.w.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					i.Add(xInput);
					i.Add(yInput);
					i.Add(zInput);
					i.Add(wInput);
					i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						field.SetValue(o, new Vector4(float.Parse(xInput.content), float.Parse(yInput.content), float.Parse(zInput.content), float.Parse(wInput.content)));
					}));
					height = 1;
				}
				else if (typeOfFieldContent.Name == "Color4")
				{
					r = Inspector.SplitRect(space, 0.4f);
					Tuple<Rect, Rect> tuple3 = Inspector.SplitRect(r.Item2, 0.25f);
					Tuple<Rect, Rect> tuple7 = Inspector.SplitRect(tuple3.Item2, 0.33f);
					Tuple<Rect, Rect> z_wRect4 = Inspector.SplitRect(tuple7.Item2, 0.5f);
					Rect xRect3 = tuple3.Item1;
					Rect yRect4 = tuple7.Item1;
					Rect zRect4 = z_wRect4.Item1;
					Rect wRect3 = z_wRect4.Item2;
					Vector4 vector4 = (Color)field.GetValue(o);
					TextGUIElement xInput = new TextGUIElement(xRect3, vector4.x.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement yInput = new TextGUIElement(yRect4, vector4.y.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement zInput = new TextGUIElement(zRect4, vector4.z.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					TextGUIElement wInput = new TextGUIElement(wRect3, vector4.w.ToString(), JMTKGUI.GUIElement.TextField, delegate()
					{
					});
					i.Add(xInput);
					i.Add(yInput);
					i.Add(zInput);
					i.Add(wInput);
					i.Add(new Button(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
					{
						field.SetValue(o, new Vector4(float.Parse(xInput.content), float.Parse(yInput.content), float.Parse(zInput.content), float.Parse(wInput.content)));
					}));
					height = 1;
				}
				else if (typeOfFieldContent.Name == "Boolean" && typeOfFieldContent.Name == "disable")
				{
					height = 1;
					try
					{
						Toggle boolIn = new Toggle(r.Item2, field.GetValue(o).ToString(), JMTKGUI.GUIElement.Toggle, delegate()
						{
						});
						i.Add(boolIn);
						i.Add(new Toggle(r.Item1, field.Name, JMTKGUI.GUIElement.Button, delegate()
						{
							field.SetValue(o, boolIn.toggleState);
						}));
					}
					catch (Exception ex2)
					{
						Debug.Log(ex2.StackTrace);
						Debug.Log("This is not how booleans work. :(");
					}
				}
			}
		}
		IL_F6E:
		return new Tuple<List<CustomGUI>, int>(i, height);
	}

	private static int iteration;

	public static int maxDepth;

	private static List<CustomGUI>[] gameObjects;

	private static List<CustomGUI> components;

	private static List<CustomGUI> componentChoice;

	public static float unitHeight = 0.02f;

	private static List<CustomGUI> componentFields = new List<CustomGUI>();
}
