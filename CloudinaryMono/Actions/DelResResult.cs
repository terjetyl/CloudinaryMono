﻿using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace CloudinaryMono.Actions
{
    [DataContract]
    public class DelResResult : BaseResult
    {
        [DataMember(Name = "deleted")]
        public Dictionary<string, string> Deleted { get; protected set; }

        /// <summary>
        /// Parses HTTP response and creates new instance of this class
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <returns>New instance of this class</returns>
        internal static DelResResult Parse(HttpWebResponse response)
        {
            return Parse<DelResResult>(response);
        }
    }
}
