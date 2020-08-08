using Deserizition;
using Sdk.Cqp.Enum;
using Sdk.Cqp.Expand;
using Sdk.Cqp.Model;
using Tool.Http;
using Native.Tool.IniConfig;
using Native.Tool.IniConfig.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Sdk.Cqp.Core
{
    public class CQP
    {
        static List<AppInfo> appInfos = new List<AppInfo>();
        static List<Requests> Requests = new List<Requests>();
        [DllExport(ExportName = "CQ_sendGroupMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_sendGroupMsg(int authcode, long groupid, IntPtr msg)
        {
            string text = Marshal.PtrToStringAnsi(msg);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=SendMsg&timeout=10";
            List<CQCode> cqCodeList = CQCode.Parse(text);
            JObject data = new JObject
            {
                {"toUser",groupid},
                {"sendToType",2},
                {"groupid",0},
                {"fileMd5","" }
            };
            bool Picflag = false, Atflag = false; ;
            foreach (var item in cqCodeList)
            {
                switch (item.Function)
                {
                    case CQFunction.At://[CQ:at,qq=xxxx]
                        {
                            if (!data.ContainsKey("atUser"))
                            {
                                data.Add("atUser", Convert.ToInt64(item.Items["qq"]));
                            }
                            else if (data["atUser"].ToString() == "0")
                                data["atUser"] = Convert.ToInt64(item.Items["qq"]);
                            Atflag = true;
                            break;
                        }
                    case CQFunction.Image:
                        {
                            if (!data.ContainsKey("content")) data.Add("content", "");
                            if (!data.ContainsKey("picBase64Buf")) data.Add("picBase64Buf", "");
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
                            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
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
                }
            }
            string filtered = Regex.Replace(text, @"\[CQ.*\]", "");
            if (!string.IsNullOrEmpty(filtered)) data["content"] = filtered;
            if (Picflag)
                data.Add("sendMsgType", "PicMsg");
            else if (Atflag)
                data.Add("sendMsgType", "AtMsg");
            else
                data.Add("sendMsgType", "TextMsg");
            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
            string pluginname = appInfos.Find(x => x.AuthCode == authcode).Name;
            LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "[↑]发送消息", $"群号:{groupid} 消息:{text}");
            SendRequest(url, data.ToString());
            return Save.MsgList.Count+1;
        }

        [DllExport(ExportName = "CQ_sendPrivateMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_sendPrivateMsg(int authCode, long qqId, IntPtr msg)
        {
            string text = Marshal.PtrToStringAnsi(msg);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=SendMsg&timeout=10";
            List<CQCode> cqCodeList = CQCode.Parse(text);
            JObject data = new JObject
            {
                {"toUser",qqId},
                {"sendToType",1},
                {"groupid",0},
                {"fileMd5","" }
            };
            bool Picflag = false, Atflag = false; ;
            foreach (var item in cqCodeList)
            {
                switch (item.Function)
                {
                    case CQFunction.At://[CQ:at,qq=xxxx]
                        {
                            if (!data.ContainsKey("atUser"))
                            {
                                data.Add("atUser", Convert.ToInt64(item.Items["qq"]));
                            }
                            else if (data["atUser"].ToString() == "0")
                                data["atUser"] = Convert.ToInt64(item.Items["qq"]);
                            Atflag = true;
                            break;
                        }
                    case CQFunction.Image:
                        {
                            if (!data.ContainsKey("content")) data.Add("content", "");
                            if (!data.ContainsKey("picBase64Buf")) data.Add("picBase64Buf", "");
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
                            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
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
                }
            }
            string filtered = Regex.Replace(text, @"\[CQ.*\]", "");
            if (!string.IsNullOrEmpty(filtered)) data["content"] = filtered;
            if (Picflag)
                data.Add("sendMsgType", "PicMsg");
            else if (Atflag)
                data.Add("sendMsgType", "AtMsg");
            else
                data.Add("sendMsgType", "TextMsg");
            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "[↑]发送消息", $"QQ:{qqId} 消息:{text}");
            SendRequest(url, data.ToString());
            return 0;
        }

        [DllExport(ExportName = "CQ_deleteMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_deleteMsg(int authCode, long msgId)
        {
            var p = Save.MsgList;
            var c = Save.MsgList.Find(x => x.MsgId == msgId);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=RevokeMsg&timeout=10";
            JObject data = new JObject
            {
                {"GroupID",c.groupid},
                {"MsgSeq",c.Seq},
                {"MsgRandom",c.MsgRandom}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            Message message = Save.MsgList.Find(x => x.MsgId == msgId);
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "撤回消息", $"群:{message.groupid} QQ:{{保留参数}} 消息:{message.text}");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "撤回消息", $"调用异常，信息{ret["Msg"]}");
            return state;
        }
        [DllExport(ExportName = "AddMsgToSave", CallingConvention = CallingConvention.StdCall)]
        public static void AddMsgToSave(Message msg)
        {
            Save.MsgList.Add(msg);
        }

        [DllExport(ExportName = "CQ_sendLikeV2", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_sendLikeV2(int authCode, long qqId, int count)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=QQZan&timeout=10";
            JObject data = new JObject
            {
                {"UserID",qqId}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "发送赞", $"向{qqId}发送了一个赞");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "发送赞", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_getCookiesV2", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getCookiesV2(int authCode, IntPtr domain)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetUserCook&timeout=10";
            JObject data = new JObject();
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "获取cookie", $"已获取个人cookie");
            return Marshal.StringToHGlobalAnsi(ret["cookies"].ToString());
        }

        [DllExport(ExportName = "CQ_getRecordV2", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getRecordV2(int authCode, IntPtr file, IntPtr format)
        {
            //待补充Record CQ码
            return Marshal.StringToHGlobalAnsi("未实现的接口");
        }

        [DllExport(ExportName = "CQ_getCsrfToken", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_getCsrfToken(int authCode)
        {
            //未找到实现接口
            return -1;
        }

        [DllExport(ExportName = "CQ_getAppDirectory", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getAppDirectory(int authCode)
        {
            string appid = appInfos.Find(x => x.AuthCode == authCode).Id;
            if (!Directory.Exists("data\\app\\"+appid))
                Directory.CreateDirectory("data\\app\\"+appid);
            string path = "data\\app\\" + appid;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return Marshal.StringToHGlobalAnsi(directoryInfo.FullName+"\\");
        }

        [DllExport(ExportName = "CQ_getLoginQQ", CallingConvention = CallingConvention.StdCall)]
        public static long CQ_getLoginQQ(int authCode)
        {
            return Save.curentQQ;
        }

        [DllExport(ExportName = "CQ_getLoginNick", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getLoginNick(int authCode)
        {
            FriendsList friendsList =JsonConvert.DeserializeObject<FriendsList>( WebAPI.GetFriendList());
            return Marshal.StringToHGlobalAnsi(friendsList.List.Where(x=>x.FriendUin==Save.curentQQ).FirstOrDefault().NickName);
        }

        [DllExport(ExportName = "CQ_setGroupKick", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupKick(int authCode, long groupId, long qqId, bool refuses)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GroupMgr&timeout=10";
            JObject data = new JObject
            {
                {"ActionType",3},
                {"GroupID",groupId},
                {"ActionUserID",qqId},
                {"Content","" }
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state=Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;            
            if(state==0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "群踢人", $"从群{groupId} 踢出了{qqId}");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "群踢人", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupBan", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupBan(int authCode, long groupId, long qqId, long time)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x570_8&timeout=10";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"ShutUpUserID",qqId},
                {"ShutTime",time/60}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "群禁言", $"在群{groupId} 禁言了{qqId} {time/60}分钟");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "群禁言", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupAdmin", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupAdmin(int authCode, long groupId, long qqId, bool isSet)
        {
            //未找到实现接口
            return -1;
        }

        [DllExport(ExportName = "CQ_setGroupSpecialTitle", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupSpecialTitle(int authCode, long groupId, long qqId, IntPtr title, long durationTime)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x8fc_2&timeout=10";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"UserID",qqId},
                {"NewTitle",Marshal.PtrToStringAnsi(title)}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "群头衔发放", $"在群{groupId} 给{qqId} 发放头衔{title}");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "群头衔发放", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupWholeBan", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupWholeBan(int authCode, long groupId, bool isOpen)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x89a_0&timeout=10";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"Switch",isOpen?1:0}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "全员禁言", $"在群{groupId} {(isOpen?"开启":"关闭")}了全员禁言");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "全员禁言", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupAnonymousBan", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupAnonymousBan(int authCode, long groupId, IntPtr anonymous, long banTime)
        {
            //未找到实现接口
            return -1;
        }

        [DllExport(ExportName = "CQ_setGroupAnonymous", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupAnonymous(int authCode, long groupId, bool isOpen)
        {
            //未找到实现接口
            return -1;
        }

        [DllExport(ExportName = "CQ_setGroupCard", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupCard(int authCode, long groupId, long qqId, IntPtr newCard)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=ModifyGroupCard&timeout=10";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"UserID",qqId},
                {"NewNick",Marshal.PtrToStringAnsi(newCard)}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["GroupID"].ToString()!="0"?0:-1);
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "群名片修改", $"在群{groupId} 修改{qqId}的群名片为{Marshal.PtrToStringAnsi(newCard)}");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "群名片修改", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupLeave", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupLeave(int authCode, long groupId, bool isDisband)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GroupMgr&timeout=10";
            JObject data = new JObject
            {
                {"ActionType",2},
                {"GroupID",groupId},
                {"ActionUserID",0},
                {"Content","" }
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "退群", $"退出了群{groupId}");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "退群", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_setDiscussLeave", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setDiscussLeave(int authCode, long disscussId)
        {
            //未找到实现接口
            return -1;
        }
        [DllExport(ExportName = "AddRequestToSave", CallingConvention = CallingConvention.StdCall)]
        public static int AddRequestToSave(Requests request)
        {
            Requests.Add(request);
            return 1;
        }
        [DllExport(ExportName = "CQ_setFriendAddRequest", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setFriendAddRequest(int authCode, IntPtr identifying, int requestType, IntPtr appendMsg)
        {
            Requests request = Requests.First(x => x.type == "FriendRequest");
            Requests.Remove(request);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=DealFriend&timeout=10";
            JToken data = JObject.Parse(request.json)["CurrentPacket"]["Data"]["EventData"];
            data["Action"] = requestType == 1 ? 2 : 3;
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "处理好友请求", $"{(requestType == 1 ? "同意" : "拒绝")}了" +
                    $"好友{Convert.ToInt64(data["UserID"].ToString())}的请求"); 
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "处理好友请求", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupAddRequestV2", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupAddRequestV2(int authCode, IntPtr identifying, int requestType, int responseType, IntPtr appendMsg)
        {
            Requests request = Requests.First(x => x.type == "GroupRequest");
            Requests.Remove(request);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=DealFriend&timeout=10";
            JToken data = JObject.Parse(request.json)["CurrentPacket"]["Data"]["EventData"];
            //JObject data = new JObject
            //{
            //    {"Seq",Convert.ToInt64(request_Des["Seq"].ToString())},
            //    {"Type",Convert.ToInt64(request_Des["Type"].ToString())},
            //    {"MsgTypeStr",request_Des["MsgTypeStr"].ToString()},
            //    {"Who",Convert.ToInt64(request_Des["Who"].ToString())},
            //    {"WhoName",request_Des["WhoName"].ToString()},
            //    {"MsgStatusStr",request_Des["MsgStatusStr"].ToString()},
            //    {"Flag_7",Convert.ToInt64(request_Des["Flag_7"].ToString())},
            //    {"Flag_8",Convert.ToInt64(request_Des["Flag_8"].ToString())},
            //    {"GroupId",Convert.ToInt64(request_Des["GroupId"].ToString())},
            //    {"GroupName",request_Des["GroupName"].ToString()},
            //    {"InviteUin",Convert.ToInt64(request_Des["InviteUin"].ToString())},
            //    {"InviteName",request_Des["InviteName"].ToString()},
            //    {"Action",responseType==1?11:21},
            //};
            data["Action"] = responseType == 1 ? 11 : 21;
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLine(pluginname, CQLogLevel.InfoSuccess, "处理加群请求", $"{(responseType == 1 ? "同意" : "拒绝")}了" +
                    $"QQ{Convert.ToInt64(data["UserID"].ToString())}" +
                    $"对群{Convert.ToInt64(data["FromGroupId"].ToString())}的请求");
            else
                LogHelper.WriteLine(pluginname, CQLogLevel.Error, "处理加群请求", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_addLog", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_addLog(int authCode, int priority, IntPtr type, IntPtr msg)
        {
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            LogHelper.WriteLine(pluginname,(CQLogLevel)System.Enum.Parse(typeof(CQLogLevel), priority.ToString()), Marshal.PtrToStringAnsi(type), Marshal.PtrToStringAnsi(msg));
            return 0;
        }

        [DllExport(ExportName = "CQ_setFatal", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setFatal(int authCode, IntPtr errorMsg)
        {
            //待找到实现方法
            return -1;
        }

        [DllExport(ExportName = "CQ_getGroupMemberInfoV2", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupMemberInfoV2(int authCode, long groupId, long qqId, bool isCache)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=friendlist.GetTroopMemberListReq&timeout=10";
            JObject data = new JObject
            {
                {"GroupUin",0},
                {"LastUin",0}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            JArray array = JArray.Parse(ret["MemberList"].ToString());
            JToken targetuser=null;
            foreach(var item in array)
            {
                if (item["MemberUin"].ToString() == qqId.ToString())
                { targetuser = item; break; }
            }
            if (targetuser == null) return (IntPtr)0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            BinaryWriterExpand.Write_Ex(binaryWriter, groupId);
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(targetuser["MemberUin"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, targetuser["NickName"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriter, targetuser["GroupCard"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(targetuser["Gender"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(targetuser["Age"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, "unkown");
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(targetuser["JoinTime"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(targetuser["LastSpeakTime"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, $"头衔{targetuser["MemberLevel"]}级");
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(targetuser["GroupAdmin"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, 0);
            BinaryWriterExpand.Write_Ex(binaryWriter, targetuser["SpecialTitle"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriter, 2051193600);
            BinaryWriterExpand.Write_Ex(binaryWriter, 1);
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(stream.ToArray()));
        }

        [DllExport(ExportName = "CQ_getGroupMemberList", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupMemberList(int authCode, long groupId)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=friendlist.GetTroopMemberListReq&timeout=10";
            JObject data = new JObject
            {
                {"GroupUin",0},
                {"LastUin",0}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            MemoryStream streamMain = new MemoryStream();
            BinaryWriter binaryWriterMain = new BinaryWriter(streamMain);
            JArray memberls = JArray.Parse(ret["MemberList"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriterMain, memberls.Count);
            foreach (var item in memberls)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(stream);
                BinaryWriterExpand.Write_Ex(binaryWriter, groupId);
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(item["MemberUin"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, item["NickName"].ToString());
                BinaryWriterExpand.Write_Ex(binaryWriter, item["GroupCard"].ToString());
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(item["Gender"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(item["Age"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, "unkown");
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(item["JoinTime"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(item["LastSpeakTime"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, $"头衔{item["MemberLevel"]}级");
                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt32(item["GroupAdmin"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, 0);
                BinaryWriterExpand.Write_Ex(binaryWriter, item["SpecialTitle"].ToString());
                BinaryWriterExpand.Write_Ex(binaryWriter, 2051193600);
                BinaryWriterExpand.Write_Ex(binaryWriter, 1);

                BinaryWriterExpand.Write_Ex(binaryWriterMain, (short)stream.Length);
                binaryWriter.Write(stream.ToArray());
            }
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(streamMain.ToArray()));
        }

        [DllExport(ExportName = "CQ_getGroupList", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupList(int authCode)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetGroupList&timeout=10";
            JObject data = new JObject
            {
                {"NextToken",""}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            MemoryStream streamMain = new MemoryStream();
            BinaryWriter binaryWriterMain = new BinaryWriter(streamMain);
            JArray grouplist = JArray.Parse(ret["TroopList"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriterMain, grouplist.Count);
            foreach(var item in grouplist)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(stream);

                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(item["GroupId"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, item["GroupName"].ToString());

                BinaryWriterExpand.Write_Ex(binaryWriterMain, (short)stream.Length);
                binaryWriter.Write(stream.ToArray());
            }
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(streamMain.ToArray()));
        }

        [DllExport(ExportName = "CQ_getStrangerInfo", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getStrangerInfo(int authCode, long qqId, bool notCache)
        {
            //未找到实现接口
            return Marshal.StringToHGlobalAnsi("未实现的接口");
        }

        [DllExport(ExportName = "CQ_canSendImage", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_canSendImage(int authCode)
        {
            return 1;
        }

        [DllExport(ExportName = "CQ_canSendRecord", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_canSendRecord(int authCode)
        {
            return 1;
        }

        [DllExport(ExportName = "CQ_getImage", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getImage(int authCode, IntPtr file)
        {
            string filename= Marshal.PtrToStringAnsi(file);
            string path = $"data\\image\\{filename}.cqimg";
            IniConfig ini = new IniConfig(path);
            ini.Object.Add(new ISection("image"));
            string picurl= ini.Object["image"]["url"];            
            if (!File.Exists($"data\\image\\"+ filename + ".cqimg"))
            {
                HttpWebClient http = new HttpWebClient { TimeOut = 10000 };
                http.DownloadFile(picurl, $"data\\image\\{filename}.jpg"); 
            }
            FileInfo fileInfo = new FileInfo($"data\\image\\{filename}.jpg");
            return Marshal.StringToHGlobalAnsi(fileInfo.FullName);
        }

        [DllExport(ExportName = "CQ_getGroupInfo", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupInfo(int authCode, long groupId, bool notCache)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetGroupList&timeout=10";
            JObject data = new JObject
            {
                {"NextToken",""}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            JArray array = JArray.Parse(ret["TroopList"].ToString());
            JToken targetgroup=null;
            foreach (var item in array)
            {
                if (item["GroupId"].ToString() == groupId.ToString())
                { targetgroup = item; break; }
            }
            if (targetgroup == null) return (IntPtr)0;
            MemoryStream stream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(targetgroup["GroupId"].ToString()));
            BinaryWriterExpand.Write_Ex(binaryWriter, targetgroup["GroupName"].ToString());
            return Marshal.StringToHGlobalAnsi("未实现的接口");
        }

        [DllExport(ExportName = "CQ_getFriendList", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getFriendList(int authCode, bool reserved)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetQQUserList&timeout=10";
            JObject data = new JObject
            {
                {"StartIndex",""}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));

            MemoryStream streamMain = new MemoryStream();
            BinaryWriter binaryWriterMain = new BinaryWriter(streamMain);
            JArray grouplist = JArray.Parse(ret["Friendlist"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriterMain, grouplist.Count);
            foreach (var item in grouplist)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(stream);

                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(item["FriendUin"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, item["NickName"].ToString());
                BinaryWriterExpand.Write_Ex(binaryWriter, item["Remark"].ToString());

                BinaryWriterExpand.Write_Ex(binaryWriterMain, (short)stream.Length);
                binaryWriter.Write(stream.ToArray());
            }
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(streamMain.ToArray()));
        }
        [DllExport(ExportName = "cq_start", CallingConvention = CallingConvention.StdCall)]
        public static bool cq_start(IntPtr path,int authcode)
        {
            string pathtext = Marshal.PtrToStringAnsi(path);
            JsonLoadSettings loadsetting = new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            };
            JObject jObject = JObject.Parse(File.ReadAllText(pathtext.Replace(".dll", ".json")), loadsetting);
            DllHelper dllHelper = new DllHelper(pathtext);
            KeyValuePair<int, string> appinfotext = dllHelper.GetAppInfo();
            AppInfo appInfo = new AppInfo(appinfotext.Value, 0, appinfotext.Key
                , jObject["name"].ToString(), jObject["version"].ToString(), Convert.ToInt32(jObject["version_id"].ToString())
                , jObject["author"].ToString(), jObject["description"].ToString(),authcode);
            appInfos.Add(appInfo);
            return true;
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
