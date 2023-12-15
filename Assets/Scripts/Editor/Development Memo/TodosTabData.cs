using System;
using System.Collections.Generic;

namespace KG.Framework
{
    /// <summary>
    /// 待办Tab页数据类
    /// </summary>
    [Serializable]
    public class TodosTabData
    {
        public List<TodoItem> todos = new List<TodoItem>(0);
    }

    /// <summary>
    /// 笔记项
    /// </summary>
    [Serializable]
    public class TodoItem
    {
        public string title;

        public string creator;

        public string description;

        public string createdTime;

        public DateTime EstimatedCompletedTime;

        public bool isCompleted;

        public bool isOverdue;

        public TodoItem()
        {
            //默认的预计完成日期为明天
            EstimatedCompletedTime = DateTime.Now.Date.AddDays(1);
        }

        public void OverdueCal()
        {
            //计算是否逾期
            isOverdue = !isCompleted && (DateTime.Now - EstimatedCompletedTime).Days > 0;
        }
    }
}