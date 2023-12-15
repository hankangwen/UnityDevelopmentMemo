using System;
using UnityEngine;
using UnityEditor;

namespace KG.Framework
{
    /// <summary>
    /// 开发备忘录
    /// </summary>
    public class DevelopmentMemoWindow : EditorWindow
    {
        [MenuItem("KGFramework/Development Memo")]
        public static void Open()
        {
            GetWindow<DevelopmentMemoWindow>("DevelopmentMemo").Show();
        }
        
        private enum Menu
        {
            Notes,
            Todos,
        }

        private Menu _menu = Menu.Notes;
        private NotesTab _notesTab;
        private TodosTab _todosTab;

        private void OnEnable()
        {
            if (_notesTab == null)
                _notesTab = new NotesTab(this);
            if (_todosTab == null)
                _todosTab = new TodosTab(this);
        }

        private void OnGUI()
        {
            OnTopGUI();
            OnBodyGUI();
        }

        private void OnDisable()
        {
            _notesTab?.OnDisable();
            _todosTab?.OnDisable();
        }

        private void OnTopGUI()
        {
            Color cacheColor = GUI.color;
            Color unSelectedColor = Color.white;
            unSelectedColor.a = 0.5f;
            GUILayout.BeginHorizontal();
            GUI.color = _menu == Menu.Notes ? Color.white : unSelectedColor;
            if (GUILayout.Button("Notes", EditorStyles.miniButtonLeft)) _menu = Menu.Notes;
            GUI.color = _menu == Menu.Todos ? Color.white : unSelectedColor;
            if (GUILayout.Button("Todos", EditorStyles.miniButtonRight)) _menu = Menu.Todos;
            GUILayout.EndHorizontal();
            GUI.color = cacheColor;
        }

        private void OnBodyGUI()
        {
            switch (_menu)
            {
                case Menu.Notes: _notesTab.OnGUI(); break;
                case Menu.Todos: _todosTab.OnGUI(); break;
                default: break;
            }
        }
    }   
}
