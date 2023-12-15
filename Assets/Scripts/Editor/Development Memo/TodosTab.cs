using System.IO;
using UnityEngine;

namespace KG.Framework
{
    /// <summary>
    /// 开发笔记Tab页
    /// </summary>
    public class TodosTab : IDevelopmentMemoTab
    {
        private readonly DevelopmentMemoWindow _window;
        /// <summary>
        /// 数据文件路径
        /// </summary>
        private readonly string _dataPath;

        public TodosTab(DevelopmentMemoWindow window)
        {
            this._window = window;
            //拼接数据文件路径
            _dataPath = Path.GetFullPath(".").Replace("\\", "/") + "/Library/DevelopmentMemo_Todos.dat";
            //判断文件是否存在
            if (File.Exists(_dataPath))
            {
                
            }
        }
        
        public void OnGUI()
        {
            throw new System.NotImplementedException();
        }

        public void OnDisable()
        {
            throw new System.NotImplementedException();
        }
    }
}