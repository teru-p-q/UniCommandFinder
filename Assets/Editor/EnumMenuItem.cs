using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class UnityMenuItem
{
    public string Path;
    public string Name;
    public int Id;
    public bool Enabled;
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

    Dictionary<string, Tuple<Type, MethodInfo, MenuItem>> _menuItems;
    Dictionary<string, Tuple<Type, MethodInfo, MenuItem>> menuItems
    {
        get
        {
            if (_menuItems == null)
            {
                _menuItems = new Dictionary<string, Tuple<Type, MethodInfo, MenuItem>>();

                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetTypes());

                var l = new List<Tuple<Type, MethodInfo, MenuItem>>();
                foreach (var type in types)
                {
                    l.AddRange(type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                            .Select(methodInfo => Tuple.Create(type, methodInfo, (methodInfo.GetCustomAttribute(typeof(MenuItem), false) as MenuItem)))
                            .Where(x => x.Item3 != null && x.Item3.validate)
                            .ToArray());
                }
                _menuItems = l.ToDictionary(x =>
                {
                    var index = x.Item3.menuItem.LastIndexOf(' ');
                    if (index != -1)
                    {
                        var a = x.Item3.menuItem.Substring(index, 2);
                        if (a == @" %" || a == @" &")
                        {
                            return x.Item3.menuItem.Substring(0, index);
                        }
                    }

                    return x.Item3.menuItem;
                }, x => x);
            }
            return _menuItems;
        }
    }

    private bool isReload = false;

    private void OnEnable()
    {
        isCompileBegin = false;
        EditorApplication.update += OnUpdate;
        if (inputText.Length > 0)
        {
            isReload = true;
        }
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
        isCompileBegin = false;
        _menuItems = null;
    }

    private bool isCompileBegin;

    private void OnUpdate()
    {
        var isCompiling = EditorApplication.isCompiling;
        if (isCompiling && !isCompileBegin)
        {
            isCompileBegin = true;
        }
    }

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

            if (inputText != after || isReload)
            {
                isReload = false;
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
            selectedIndex = ItemListField(selectedIndex, displayMenuItems.ToFormatItems(menuItems, inputText));
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
    public static string[] ToFormatItems(this List<UnityMenuItem> items, Dictionary<string, Tuple<Type, MethodInfo, MenuItem>> methods, string filter)
    {
        var b = new StringBuilder();

        return items
            .Select(x =>
            {
                var command = x.Name;
                Tuple<Type, MethodInfo, MenuItem> item;

                if (methods.TryGetValue(x.Path + command, out item))
                {
                    x.Enabled = (bool)item.Item2.Invoke(null, null);
                }

                b.Clear();

                if (!x.Enabled)
                {
                    b.Append("<color=grey>");
                }

                while (true)
                {
                    var index = command.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase);
                    if (index == -1)
                    {
                        if (command.Length > 0)
                        {
                            b.Append(command);
                        }
                        break;
                    }

                    b.Append(command.Substring(0, index));
                    b.Append("<b>");
                    b.Append(command.Substring(index, filter.Length));
                    b.Append("</b>");

                    command = command.Substring(index + filter.Length);
                }

                if (!x.Enabled)
                {
                    b.Append("</color>");
                }

                return b.ToString();
            })
            .ToArray();
    }
}
