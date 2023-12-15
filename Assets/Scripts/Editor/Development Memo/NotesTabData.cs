using System;
using System.Collections.Generic;

namespace KG.Framework
{
    /// <summary>
    /// 笔记Tab页数据类
    /// </summary>
    [Serializable]
    public class NotesTabData
    {
        /// <summary>
        /// 笔记列表
        /// </summary>
        public List<NoteItem> notes = new List<NoteItem>(0);
    }

    /// <summary>
    /// 笔记项
    /// </summary>
    [Serializable]
    public class NoteItem
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createdTime;

        /// <summary>
        /// 标题
        /// </summary>
        public string title;

        /// <summary>
        /// 作者
        /// </summary>
        public string author;

        /// <summary>
        /// 内容
        /// </summary>
        public string content;
    }
}