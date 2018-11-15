using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    /// <summary>
    /// 用户属性
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int AppUserId { get; set; }

        public string Key { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }
    }
}
