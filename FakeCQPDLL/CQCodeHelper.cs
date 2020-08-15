using Native.Tool.IniConfig;
using Newtonsoft.Json.Linq;
using Sdk.Cqp.Enum;
using Sdk.Cqp.Expand;
using Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Sdk.Cqp.Core
{
    public static class CQCodeHelper
    {
        public static void Progeress(List<CQCode> cqCodeList,ref JObject data,ref string text)
        {
            bool Picflag = false, Voiceflag = false,Atflag=false;
            //List<long> atQQs = new List<long>();
            foreach (var item in cqCodeList)
            {
                switch (item.Function)
                {
                    case CQFunction.At://[CQ:at,qq=xxxx]
                        {
                            if (!data.ContainsKey("content")) data.Add("content","");
                            text = text.Replace(item.ToSendString(), $"[ATUSER({Convert.ToInt64(item.Items["qq"])})]");
                            //atQQs.Add(Convert.ToInt64(item.Items["qq"]));
                            //Atflag = true;
                            break;
                        }
                    case CQFunction.Image:
                        {
                            if (!data.ContainsKey("content")) data.Add("content", "");
                            if (!data.ContainsKey("picBase64Buf")) data.Add("picBase64Buf", "");
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
                            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
                            if (item.Items.ContainsKey("url"))
                                data["picUrl"] = item.Items["url"];
                            else if (item.Items.ContainsKey("file"))
                            {
                                if (File.Exists("data\\image\\" + item.Items["file"] + ".cqimg"))
                                {
                                    IniConfig ini = new IniConfig("data\\image\\" + item.Items["file"] + ".cqimg"); ini.Load();
                                    data["picUrl"] = ini.Object["image"]["url"].ToString();
                                    Picflag = true;
                                    break;
                                }
                                string path = item.Items["file"], base64buf = string.Empty;
                                if (File.Exists("data\\image\\" + path))
                                {
                                    base64buf = BinaryReaderExpand.ImageToBase64("data\\image\\" + path);
                                }
                                data["picBase64Buf"] = base64buf;
                            }
                            else if (item.Items.ContainsKey("md5"))
                            {
                                data["fileMd5"] = item.Items["file"];
                            }
                            Picflag = true;
                            break;
                        }
                    case CQFunction.Record:
                        {
                            if (!data.ContainsKey("content")) data.Add("content", "");
                            if (!data.ContainsKey("voiceBase64Buf")) data.Add("voiceBase64Buf", "");
                            if (!data.ContainsKey("voiceUrl")) data.Add("voiceUrl", "");
                            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
                            if (item.Items.ContainsKey("file"))
                            {
                                string voicepath = $"data\\record\\{item.Items["file"]}";
                                if (File.Exists(voicepath))
                                {
                                    try
                                    {
                                        IniConfig ini = new IniConfig(voicepath);ini.Load();
                                        data["voiceUrl"] = ini.Object["record"]["url"].ToString();
                                    }
                                    catch
                                    {
                                        string base64Str;
                                        using (FileStream fsRead = new FileStream(voicepath, FileMode.Open))
                                        {
                                            int fsLen = (int)fsRead.Length;
                                            byte[] heByte = new byte[fsLen];
                                            int r = fsRead.Read(heByte, 0, heByte.Length);
                                            base64Str = Convert.ToBase64String(heByte);
                                        }
                                        data["voiceBase64Buf"] = base64Str;
                                    }
                                    Voiceflag = true;
                                }
                            }
                            break;
                        }
                    case CQFunction.Face:
                        {
                            text = Regex.Replace(text, "\\[CQ:face,id=(\\d*)\\]", "[表情$1]");
                            break;
                        }
                    case CQFunction.Emoji:
                        {
                            int src =Convert.ToInt32(item.Items["id"]);
                            text = text.Replace(item.ToSendString(), Encoding.UTF32.GetString(BitConverter.GetBytes(src)));
                            break;
                        }
                }
            }
            string filtered = Regex.Replace(text, @"\[CQ.*\]", "");
            if (!string.IsNullOrEmpty(filtered)) data["content"] = filtered;
            if (Picflag)
                data.Add("sendMsgType", "PicMsg");
            else if (Voiceflag)
                data.Add("sendMsgType", "VoiceMsg");
            else
                data.Add("sendMsgType", "TextMsg");
        }
    }
}
