using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace KG.Framework
{
    /// <summary>
    /// 开发笔记Tab页
    /// </summary>
    public class NotesTab : IDevelopmentMemoTab
    {
        private readonly DevelopmentMemoWindow _window;
        //数据文件路径
        private readonly string _dataPath;
        //数据类
        [SerializeField] private NotesTabData _data;
        //笔记列表的宽度
        private float _listRectWidth = 280f;
        //左右分割线区域
        private Rect _splitterRect;
        //是否正在拖拽分割线
        private bool _isDragging;
        //列表滚动值
        private Vector2 _listScroll;
        //笔记详情滚动值
        private Vector2 _detailScroll;
        //当前选中的笔记
        private NoteItem _currentNote;
        //标题的最大长度
        private const int TitleLengthLimit = 20;
        //检索内容
        private string _searchContent;

        public NotesTab(DevelopmentMemoWindow window)
        {
            this._window = window;
            //拼接数据文件路径
            _dataPath = Path.GetFullPath(".").Replace("\\", "/") + "/Library/DevelopmentMemo_Notes.dat";
            //判断文件是否存在
            if (File.Exists(_dataPath))
            {
                using (FileStream fs = File.Open(_dataPath, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //反序列化
                    var deserialize = bf.Deserialize(fs);
                    if (deserialize != null)
                        _data = deserialize as NotesTabData;
                    //反序列化失败
                    if (_data == null)
                    {
                        //删除无效文件 并初始化一个新的数据类
                        File.Delete(_dataPath);
                        _data = new NotesTabData();
                    }
                }
            }
            else
            {
                _data = new NotesTabData();
            }
        }

        public void OnGUI()
        {
            OnTopGUI();
            OnBodyGUI();
        }

        private void OnTopGUI()
        {
            GUILayout.BeginHorizontal();
            //排序按钮
            GUI.enabled = _data != null && _data.notes.Count > 0;
            if (GUILayout.Button("Sort", EditorStyles.toolbarDropDown, GUILayout.Width(50f)))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent("Name ↓"), false,
                    () => _data.notes = _data.notes.OrderBy(m => m.title).ToList());
                gm.AddItem(new GUIContent("Name ↑"), false,
                    () => _data.notes = _data.notes.OrderByDescending(m => m.title).ToList());
                gm.AddItem(new GUIContent("Created Time ↓"), false,
                    () => _data.notes = _data.notes.OrderBy(m => m.createdTime).ToList());
                gm.AddItem(new GUIContent("Created Time \u2191"), false,
                    () => _data.notes = _data.notes.OrderByDescending(m => m.createdTime).ToList());
                gm.ShowAsContext();
            }

            GUI.enabled = true;
            //检索输入框
            _searchContent = GUILayout.TextField(_searchContent, EditorStyles.toolbarSearchField);
            //当点击鼠标且鼠标位置不在输入框中时 取消控件的聚焦
            //if (Event.current.type == EventType.MouseDown && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            //{
            //    GUI.FocusControl(null);
            //    window.Repaint();
            //}
            GUILayout.EndHorizontal();
        }

        private void OnBodyGUI()
        {
            GUILayout.BeginHorizontal();
            {
                //左侧列表
                GUILayout.BeginVertical(GUILayout.Width(_listRectWidth));
                OnLeftGUI();
                GUILayout.EndVertical();
                
                //分割线
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.MaxWidth(5f));
                GUILayout.Box(string.Empty, "EyeDropperVerticalLine", GUILayout.ExpandHeight(true));
                GUILayout.EndVertical();
                _splitterRect = GUILayoutUtility.GetLastRect();
                
                //右侧详情
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                OnRightGUI();
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            if (Event.current != null)
            {
                //光标
                EditorGUIUtility.AddCursorRect(_splitterRect, MouseCursor.ResizeHorizontal);
                switch (Event.current.rawType)
                {
                    //开始拖拽分割线
                    case EventType.MouseDown:
                        _isDragging = _splitterRect.Contains(Event.current.mousePosition);
                        break;
                    case EventType.MouseDrag:
                        if (_isDragging)
                        {
                            _listRectWidth += Event.current.delta.x;
                            _listRectWidth = Mathf.Clamp(_listRectWidth, _window.position.width * 0.3f,
                                _window.position.width * 0.8f);
                            _window.Repaint();
                        }
                        break;
                    //结束拖拽分割线
                    case EventType.MouseUp:
                        if (_isDragging) 
                            _isDragging = false;
                        break;
                }
            }
        }

        private void OnLeftGUI()
        {
            //滚动视图
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            {
                //遍历笔记列表
                for (int i = 0; i < _data.notes.Count; i++)
                {
                    NoteItem note = _data.notes[i];
                    //如果检索输入框不为空，判断是否符合检索内容
                    if(!string.IsNullOrEmpty(_searchContent) && !note.title.ToLower().Contains(_searchContent.ToLower())) continue;
                    
                    GUILayout.BeginHorizontal(_currentNote==note? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop");
                    GUILayout.Label(note.title);
                    GUILayout.EndHorizontal();
                    //鼠标点击选中当前项
                    if (Event.current.type == EventType.MouseDown &&
                        GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        if (_currentNote != note)
                        {
                            GUI.FocusControl(null);
                            _currentNote = note;
                            _window.Repaint();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            
            GUILayout.FlexibleSpace();
            
            //创建新的笔记
            if (GUILayout.Button("Create New", EditorStyles.miniButton))
            {
                var note = new NoteItem()
                {
                    title = "New Note",
                    createdTime = DateTime.Now.ToString(),
                };
                //添加到数据列表
                _data.notes.Add(note);
            }
            GUILayout.Space(2.5f);
        }

        private void OnRightGUI()
        {
            if (_currentNote == null) return;

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            //标题
            GUILayout.Label("标题：", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            string newTitle = EditorGUILayout.TextField(_currentNote.title, EditorStyles.label);
            if (newTitle != _currentNote.title)
            {
                //长度限制
                if (newTitle.Length > 0 && newTitle.Length <= TitleLengthLimit)
                    _currentNote.title = newTitle;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_currentNote.title.Length}/{TitleLengthLimit}", EditorStyles.miniBoldLabel);
            GUILayout.EndHorizontal();
            
            //日期
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_currentNote.createdTime, EditorStyles.miniBoldLabel);
            GUILayout.EndHorizontal();
            
            //作者
            GUILayout.Label("作者：", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            _currentNote.author = EditorGUILayout.TextField(_currentNote.author, EditorStyles.label);
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            
            //内容
            GUILayout.Label("内容：", EditorStyles.boldLabel);
            _currentNote.content = EditorGUILayout.TextArea(_currentNote.content,
                GUILayout.MaxWidth(_window.position.width - _listRectWidth - 15f), GUILayout.MinHeight(200f));
            
            //删除
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog("Notice", "Whether to delete the note ?", "Confirm", "Cancel"))
                {
                    _data.notes.Remove(_currentNote);
                    _currentNote = null;
                    _window.Repaint();
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
        }

        public void OnDisable()
        {
            try
            {
                //写入数据文件进行保存
                using (FileStream fs = File.Create(_dataPath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //序列化
                    bf.Serialize(fs, _data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }
}