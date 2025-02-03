using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Numeira;

[InitializeOnLoad]
internal static class GameObjectContextMenuManager
{
    static GameObjectContextMenuManager()
    {
        EditorApplication.delayCall += OnDelayCall;
    }

    private static HashSet<string>? MenuItemRoots;

    private const string GameObjectMenuRoot = "GameObject/";
    private const string MenuPathRoot = "Manage Items";

    private static void OnDelayCall()
    {
        // AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)).Where(x => x.GetCustomAttribute<MenuItem>() != null);
        var items = new List<MenuItem>();
        items.Clear();
        foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach(var type in assembly.GetTypes())
            {
                foreach(var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attributes = Attribute.GetCustomAttributes(method, typeof(MenuItem), false);
                    foreach(var attribute in attributes.Cast<MenuItem>())
                    {
                        if (attribute.menuItem.StartsWith(GameObjectMenuRoot))
                            items.Add(attribute);
                    }
                }
            }
        }
        items.Sort((x, y) => x.priority.CompareTo(y.priority));
        MenuItemRoots = items.Select(x =>
        {
            var name = x.menuItem.AsSpan(GameObjectMenuRoot.Length);
            if (name.IndexOf('/') is int index and not -1)
            {
                name = name[..index];
            }
            if (name.IndexOfAny("%$&_") is int index2 and not -1 && name[index2 - 1] == ' ')
            {
                name = name[..(index2 - 1)];
            }
            return name.ToString();
        }).ToHashSet();

        SceneHierarchyHooks.addItemsToGameObjectContextMenu -= OnGameObjectContextMenu;
        SceneHierarchyHooks.addItemsToGameObjectContextMenu += OnGameObjectContextMenu;
    }

    //[MenuItem(GameObjectMenuRoot + MenuPathRoot + "/" + "Check All", false, int.MaxValue)]
    internal static void CheckAll() 
    {
        var config = GameObjectContextMenuManagerConfiguration.instance;
        config.DisabledItems.Clear();
        config.Save();
    }

    //[MenuItem(GameObjectMenuRoot + MenuPathRoot + "/" + "Unckeck All", false, int.MaxValue)]
    internal static void UncheckAll()
    {
        if (MenuItemRoots == null)
            return;

        var config = GameObjectContextMenuManagerConfiguration.instance;
        foreach(var x in MenuItemRoots)
        {
            config.DisabledItems.Add(x);
        }
        config.Save();
    }

    private static void OnGameObjectContextMenu(GenericMenu menu, GameObject @object)
    {
        if (MenuItemRoots == null)
            return;

        var list = Unsafe.As<Tuple<List<MenuItemItem>>>(menu).Item1;

        list.RemoveAll(x =>
        {
            if (x.Content == null)
                return false;

            var text = x.Content.text;
            if (string.IsNullOrEmpty(text))
                return false;

            if (text.IndexOf('/') is int index and not -1)
            {
                text = text[..index];
            }

            return GameObjectContextMenuManagerConfiguration.instance.DisabledItems.Contains(text);
        });

        menu.AddSeparator("");

        void AddMenu(string path, GenericMenu.MenuFunction callback, bool enable)
        {
            if (!enable)
            {
                menu.AddDisabledItem(new(path));
                return;
            }
            menu.AddItem(new(path), false, callback);
        }

        AddMenu($"{MenuPathRoot}/Check All", CheckAll, GameObjectContextMenuManagerConfiguration.instance.DisabledItems.Count != 0);
        AddMenu($"{MenuPathRoot}/Uncheck All", UncheckAll, GameObjectContextMenuManagerConfiguration.instance.DisabledItems.Count != MenuItemRoots.Count);

        menu.AddSeparator(MenuPathRoot + "/");
        StringBuilder sb = new();
        foreach(var name in MenuItemRoots)
        {
            sb.Clear();
            sb.Append(MenuPathRoot);
            sb.Append("/");
            sb.Append(name);
            var path = sb.ToString();

            menu.AddItem(new(path), !GameObjectContextMenuManagerConfiguration.instance.DisabledItems.Contains(name), static context => 
            {
                var name = (context as string)!;
                var items = GameObjectContextMenuManagerConfiguration.instance.DisabledItems;
                if (items.Contains(name))
                    items.Remove(name);
                else
                    items.Add(name);

                GameObjectContextMenuManagerConfiguration.instance.Save();
            }, name);
        }

        //list.RemoveAll(x => x.Content?.text.EndsWith(TemporaryMenuItemName) ?? false);
    }

    internal sealed class MenuItemItem
    {
        public GUIContent? Content;
        public bool Separator;
        public bool On;
        public GenericMenu.MenuFunction? Func;
        public GenericMenu.MenuFunction2? Func2;
        public object? UserData;
    }
}
