﻿using System;

namespace User.API.Models
{
    public class BPFile
    {

        /// <summary>
        /// BP Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 原始文件地址
        /// </summary>
        public string OriginFilePath { get; set; }

        /// <summary>
        /// 格式转换后的地址
        /// </summary>
        public string FormatFilePath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}