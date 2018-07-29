using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class UnityMenuItem
{
    public string Path;
    public string Name;
    public int Id;
}

[Serializable]
public class EnumMenuItem : EditorWindow
{
    [MenuItem("Window/Command Finder")]
    static void Open()
    {
        var w = GetWindow<EnumMenuItem>();
        w.titleContent = new GUIContent("Finder");
    }

    string inputText = "";
    int selectedIndex = 0;
    int maxIndex = -1;
    bool isKeyDown = false;
    KeyCode downKey = KeyCode.None;
    List<UnityMenuItem> allMenuItems = null;
    List<UnityMenuItem> displayMenuItems = new List<UnityMenuItem>();
    string reserveExec = "";

    /// <summary>
    /// 
    /// </summary>
    void Input()
    {
        var e = Event.current;
        var keyCode = e.keyCode;
        var type = e.type;

        if (type == EventType.KeyDown)
        {
            if (keyCode == KeyCode.Return && selectedIndex != -1)
            {
                isKeyDown = true;
                downKey = keyCode;
            }

            if (keyCode == KeyCode.DownArrow)
            {
                isKeyDown = true;
                downKey = keyCode;

                if (selectedIndex < maxIndex - 1)
                {
                    selectedIndex++;
                }
            }

            if (keyCode == KeyCode.UpArrow)
            {
                isKeyDown = true;
                downKey = keyCode;

                if (selectedIndex > 0)
                {
                    selectedIndex--;
                }
            }
        }
        else if (isKeyDown && type == EventType.KeyUp)
        {
            if (keyCode == downKey && (keyCode == KeyCode.Return || keyCode == KeyCode.UpArrow || keyCode == KeyCode.DownArrow))
            {
                isKeyDown = false;
            }

            if (keyCode == KeyCode.Return && selectedIndex != -1)
            {
                selectedIndex = ExecuteMenuItem(displayMenuItems, selectedIndex, maxIndex);
            }
        }

        if (e.isMouse && e.clickCount > 1)
        {
            selectedIndex = ExecuteMenuItem(displayMenuItems, selectedIndex, maxIndex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        var e = Event.current;

        if (allMenuItems == null)
        {
            allMenuItems = new AllMenuItem().EnumItems();
        }

        var beforeIndex = selectedIndex;

        Input();

        //
        EditorGUILayout.BeginVertical();
        {
            //
            GUI.SetNextControlName("Finder");
            var after = InputTextField(inputText);

            if (inputText != after)
            {
                inputText = after;

                displayMenuItems = allMenuItems.Where(x => x.Name.ToLower().IndexOf(after) != -1).ToList();
                maxIndex = displayMenuItems.Count();
                if (selectedIndex >= maxIndex)
                {
                    selectedIndex = -1;
                }

                if (after.Length == 0)
                {
                    displayMenuItems.Clear();
                }
            }

            // 
            selectedIndex = ItemListField(selectedIndex, displayMenuItems.ToFormatItems(inputText));
            // 
            PathLabelField(displayMenuItems, selectedIndex);
        }
        EditorGUILayout.EndVertical();

        GUI.FocusControl("Finder");

        if (beforeIndex != selectedIndex)
        {
            HandleUtility.Repaint();
        }

        Execute();
    }

    /// <summary>
    /// 
    /// <returns></returns>
    string InputTextField(string inputText)
    {
        EditorGUILayout.BeginVertical(GUILayout.Height(30));
        GUILayout.Space(10);
        var after = EditorGUILayout.TextField(inputText);
        EditorGUILayout.EndVertical();
        return after;
    }

    Vector2 scrollPos;

    /// <summary>
    /// 
    /// </summary>
    int ItemListField(int currentIndex, string[] strings)
    {
        EditorGUILayout.BeginVertical();
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUI.skin.box);

        var style = new GUIStyle("PreferencesKeysElement");
        style.richText = true;

        var selectedIndex = GUILayout.SelectionGrid(currentIndex, strings, 1, style);
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        return selectedIndex;
    }

    /// <summary>
    ///
    /// </summary>
    void PathLabelField(List<UnityMenuItem> items, int index)
    {
        EditorGUILayout.BeginVertical(GUI.skin.label, GUILayout.Height(30));
        string path = "";

        if (index > -1 && index < maxIndex && items.Count() > index)
        {
            path = items[index].Path;
        }

        EditorGUILayout.LabelField(path);
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    int ExecuteMenuItem(List<UnityMenuItem> items, int index, int maxIndex)
    {
        if (index < 0 || maxIndex <= index)
        {
            return -1;
        }

        var item = items[index];
        reserveExec = item.Path + item.Name;

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    void Execute()
    {
        if (reserveExec != "")
        {
            var path = reserveExec;
            reserveExec = "";
            EditorApplication.ExecuteMenuItem(path);
            GUIUtility.ExitGUI();
        }
    }
}

public static class MenuItemsExtensions
{
    public static string[] ToFormatItems(this List<UnityMenuItem> items, string filter)
    {
        var b = new StringBuilder();

        return items
            .Select(x =>
            {
                var s = x.Name;
                b.Clear();

                while (true)
                {
                    var index = s.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase);
                    if (index == -1)
                    {
                        if (s.Length > 0)
                        {
                            b.Append(s);
                        }
                        break;
                    }

                    b.Append(s.Substring(0, index));
                    b.Append("<b>");
                    b.Append(s.Substring(index, filter.Length));
                    b.Append("</b>");

                    s = s.Substring(index + filter.Length);
                }

                return b.ToString();
            })
            .ToArray();
    }
}
