using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

namespace CloudinaryMono.Actions
{
    /// <summary>
    /// Results of tags management
    /// </summary>
    [DataContract]
    public class MultiResult : BaseResult
    {
        [DataMember(Name = "url")]
        public Uri Uri { get; protected set; }

        [DataMember(Name = "secure_url")]
        public Uri SecureUri { get; protected set; }

        [DataMember(Name = "public_id")]
        public string PublicId { get; protected set; }

        [DataMember(Name = "version")]
        public string Version { get; protected set; }

        /// <summary>
        /// Parses HTTP response and creates new instance of this class
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <returns>New instance of this class</returns>
        internal static MultiResult Parse(HttpWebResponse response)
        {
            return Parse<MultiResult>(response);
        }
    }
}
