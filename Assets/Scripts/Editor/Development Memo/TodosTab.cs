using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

namespace KG.Framework
{
    /// <summary>
    /// 待办事项Tab页
    /// </summary>
    public class TodosTab : IDevelopmentMemoTab
    {
        private readonly DevelopmentMemoWindow _window;
        //数据文件路径
        private readonly string _dataPath;
        //数据类
        [SerializeField] private TodosTabData _data;
        //笔记列表的宽度
        private float _listRectWidth = 280f;
        //左右分割线区域
        private Rect _splitterRect;
        //是否正在拖拽分割线
        private bool _isDragging;
        //列表滚动值
        private Vector2 _listScroll;
        //待办详情滚动值
        private Vector2 _detailScroll;
        //当前选中的待办项
        private TodoItem _currentTodo;
        //标题的最大长度
        private const int TitleLengthLimit = 20;
        //检索内容
        private string _searchContent;

        public TodosTab(DevelopmentMemoWindow window)
        {
            this._window = window;
            //拼接数据文件路径
            _dataPath = Path.GetFullPath(".").Replace("\\", "/") + "/Library/DevelopmentMemo_Todos.dat";
            //判断文件是否存在
            if (File.Exists(_dataPath))
            {
                using (FileStream fs = File.Open(_dataPath, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    //反序列化
                    var deserialize = bf.Deserialize(fs);
                    if(deserialize!=null)
                        _data=deserialize as TodosTabData;
                    //反序列化失败 数据为空
                    if (_data == null)
                    {
                        //删除无效数据文件 并初始化一个新的数据类
                        File.Delete(_dataPath);
                        _data = new TodosTabData();
                    }
                    else
                    {
                        //默认按照创建时间排序
                        _data.todos = _data.todos.OrderBy(m => m.createdTime).ToList();
                    }
                }
            }
            else
            {
                _data = new TodosTabData();
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
            if (GUILayout.Button("Sort", EditorStyles.toolbarDropDown, GUILayout.Width(50f)))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent("Name ↓"), false, 
                    () => _data.todos = _data.todos.OrderBy(m => m.title).ToList());
                gm.AddItem(new GUIContent("Name ↑"), false, 
                    () => _data.todos = _data.todos.OrderByDescending(m => m.title).ToList());
                gm.AddItem(new GUIContent("Created Time ↓"), false, 
                    () => _data.todos = _data.todos.OrderBy(m => m.createdTime).ToList());
                gm.AddItem(new GUIContent("Created Time ↑"), false, 
                    () => _data.todos = _data.todos.OrderByDescending(m => m.createdTime).ToList());
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
                            //限制其最大最小值
                            _listRectWidth = Mathf.Clamp(_listRectWidth, _window.position.width * .3f, _window.position.width * .8f);
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
                for (int i = 0; i < _data.todos.Count; i++)
                {
                    var todo = _data.todos[i];
                    //如果检索输入框不为空 判断是否符合检索内容
                    if (!string.IsNullOrEmpty(_searchContent) && !todo.title.ToLower().Contains(_searchContent.ToLower())) continue;
                    GUILayout.BeginHorizontal(_currentTodo == todo ? "MeTransitionSelectHead" : "ProjectBrowserHeaderBgTop");
                    GUILayout.Label(todo.title);
                    GUILayout.FlexibleSpace();
                    if (todo.isCompleted) GUILayout.Label("✓");
                    else if (todo.isOverdue) GUILayout.Label("！");
                    GUILayout.EndHorizontal();
                    //鼠标点击选中当前项
                    if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        if (_currentTodo != todo)
                        {
                            GUI.FocusControl(null);
                            _currentTodo = todo;
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
                var note = new TodoItem()
                {
                    title = "New Todo",
                    createdTime = DateTime.Now.ToString(),
                };
                //添加到数据列表
                _data.todos.Add(note);
            }
            GUILayout.Space(2.5f);
        }

        private void OnRightGUI()
        {
            if (_currentTodo == null) return;

            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);
            //标题
            GUILayout.Label("标题：", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            string newTitle = EditorGUILayout.TextField(_currentTodo.title, EditorStyles.label);
            if (newTitle != _currentTodo.title)
            {
                //长度限制
                if (newTitle.Length > 0 && newTitle.Length <= TitleLengthLimit)
                    _currentTodo.title = newTitle;
            }
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("{0}/{1}", _currentTodo.title.Length, TitleLengthLimit), EditorStyles.miniBoldLabel);
            GUILayout.EndHorizontal();

            //日期
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(_currentTodo.createdTime, EditorStyles.miniBoldLabel);
            GUILayout.EndHorizontal();

            //创建人
            GUILayout.Label("创建人：", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            _currentTodo.creator = EditorGUILayout.TextField(_currentTodo.creator, EditorStyles.label);
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);

            //描述
            GUILayout.Label("待办描述：", EditorStyles.boldLabel);
            _currentTodo.description = EditorGUILayout.TextArea(_currentTodo.description,
                GUILayout.MaxWidth(_window.position.width - _listRectWidth - 15f), GUILayout.MinHeight(100f));
            GUILayout.Space(20f);

            //当前状态
            GUILayout.Label("当前状态：", EditorStyles.boldLabel);
            if (GUILayout.Button(_currentTodo.isCompleted ? "已完成" : "未完成", "DropDownButton"))
            {
                GenericMenu gm = new GenericMenu();
                gm.AddItem(new GUIContent("未完成"), !_currentTodo.isCompleted, () => { _currentTodo.isCompleted = false; _currentTodo.OverdueCal(); });
                gm.AddItem(new GUIContent("已完成"), _currentTodo.isCompleted, () => { _currentTodo.isCompleted = true; _currentTodo.OverdueCal(); });
                gm.ShowAsContext();
            }
            GUILayout.Space(20f);

            //未完成
            if (!_currentTodo.isCompleted)
            {
                EditorGUI.BeginChangeCheck();
                //预计完成日期
                GUILayout.Label("预计完成日期：", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                //年
                if (GUILayout.Button("<", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddYears(-1);
                GUILayout.Label(_currentTodo.EstimatedCompletedTime.Year.ToString(), GUILayout.Width(35f));
                if (GUILayout.Button(">", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddYears(1);
                GUILayout.Label("年", GUILayout.Width(20f));
                GUILayout.EndHorizontal();
                //月
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddMonths(-1);
                GUILayout.Label(_currentTodo.EstimatedCompletedTime.Month.ToString(), GUILayout.Width(35f));
                if (GUILayout.Button(">", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddMonths(1);
                GUILayout.Label("月", GUILayout.Width(20f));
                GUILayout.EndHorizontal();
                //日
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddDays(-1);
                GUILayout.Label(_currentTodo.EstimatedCompletedTime.Day.ToString(), GUILayout.Width(35f));
                if (GUILayout.Button(">", GUILayout.Width(18f)))
                    _currentTodo.EstimatedCompletedTime = _currentTodo.EstimatedCompletedTime.AddDays(1);
                GUILayout.Label("日", GUILayout.Width(20f));
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                    _currentTodo.OverdueCal();
            }

            //删除
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete"))
            {
                if (EditorUtility.DisplayDialog("Notice", "Whether to delete the todo item ?", "Confirm", "Cancle"))
                {
                    _data.todos.Remove(_currentTodo);
                    _currentTodo = null;
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