using Deserizition;
using Sdk.Cqp.Enum;
using Sdk.Cqp.Expand;
using Sdk.Cqp.Model;
using Tool.Http;
using Native.Tool.IniConfig;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdk.Cqp.Core
{
    public static class WebAPI
    {
        public static string GetGroupMemberList(long groupid)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetGroupUserList&timeout=10";
            JObject data = new JObject
            {
                {"GroupUin",groupid},
                {"LastUin",0},
            };
            return SendRequest(url, data.ToString());
        }
        public static string GetFriendList()
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetQQUserList&timeout=10";
            JObject data = new JObject
            {
                {"StartIndex",0}
            };
            return SendRequest(url, data.ToString());
        }
        public static string SendRequest(string url, string data)
        {
            HttpWebClient http = new HttpWebClient
            {
                ContentType = "application/json",
                Accept = "*/*",
                KeepAlive = true,
                Method = "POST",
                Encoding = Encoding.UTF8,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36",
                TimeOut = 10000
            };
            return http.UploadString(url, data.ToString());
        }
    }
}
