using Deserizition;
using Launcher.Sdk.Cqp.Expand;
using Launcher.Sdk.Cqp.Model;
using Tool.Http;
using Native.Tool.IniConfig;
using Native.Tool.IniConfig.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Launcher.Sdk.Cqp.Core;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace CQP
{
    public class CQP
    {
        static List<AppInfo> appInfos = new List<AppInfo>();
        static List<Requests> Requests = new List<Requests>();
        static Encoding GB18030 = Encoding.GetEncoding("GB18030");

        [DllExport(ExportName = "CQ_sendGroupMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_sendGroupMsg(int authcode, long groupid, IntPtr msg)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            string text = msg.ToString(GB18030);
            string textBackup = msg.ToString(GB18030);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=SendMsg";
            JObject data;
            List<CQCode> cqCodeList = CQCode.Parse(text);
            if (text.Contains("[CQ:image") || text.Contains("[CQ:record"))
            {
                data = new JObject
                {
                    {"toUser",groupid},
                    {"sendToType",2},
                    {"groupid",0},
                    {"fileMd5","" },
                    {"atUser",0 }
                };
            }
            else
            {
                url += "V2";
                data = new JObject
                {
                    {"ToUserUid",groupid},
                    {"SendToType",2}
                };
            }
            CQCodeHelper.Progeress(cqCodeList, ref data, ref text);
            string pluginname = appInfos.Find(x => x.AuthCode == authcode).Name;
            if (Save.TestPluginsList.Any(x => x == pluginname))
            {
                Save.TestPluginChatter.Invoke(new System.Windows.Forms.MethodInvoker(() =>
                {
                    Save.TestPluginChatter.SelectionColor = Color.Green;
                    Save.TestPluginChatter.AppendText($"[{DateTime.Now:HH:mm:ss}] 插件发送消息: {textBackup}\n");
                }));
                return 0;
            }
            else
            {
                int logid = LogHelper.WriteLog(LogLevel.InfoSend, pluginname, "[↑]发送消息", $"群号:{groupid} 消息:{msg.ToString(GB18030)}", "处理中...");
                WebAPI.SendRequest(url, data.ToString());
                sw.Stop();
                LogHelper.UpdateLogStatus(logid, $"√ {sw.ElapsedMilliseconds/ (double)1000:f2} s");
                return Save.MsgList.Count + 1;
            }
        }

        [DllExport(ExportName = "CQ_sendPrivateMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_sendPrivateMsg(int authCode, long qqId, IntPtr msg)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string text = msg.ToString(GB18030);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=SendMsg";
            JObject data;
            List<CQCode> cqCodeList = CQCode.Parse(text);
            if (text.Contains("[CQ:image"))
            {
                data = new JObject
                {
                    {"toUser",qqId},
                    {"sendToType",1},
                    {"groupid",0},
                    {"fileMd5","" },
                    {"atUser",0 }
                };
            }
            else
            {
                url += "V2";
                data = new JObject
                {
                    {"ToUserUid",qqId},
                    {"SendToType",1}
                };
            }
            CQCodeHelper.Progeress(cqCodeList, ref data, ref text);
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            int logid = LogHelper.WriteLog(LogLevel.InfoSend, pluginname, "[↑]发送好友消息", $"QQ:{qqId} 消息:{msg.ToString(GB18030)}", "处理中...");
            WebAPI.SendRequest(url, data.ToString());
            sw.Stop();
            LogHelper.UpdateLogStatus(logid, $"√ {sw.ElapsedMilliseconds/ (double)1000:f2} s");
            return 0;
        }

        [DllExport(ExportName = "CQ_deleteMsg", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_deleteMsg(int authCode, long msgId)
        {
            var c = Save.MsgList.Find(x => x.MsgId == msgId);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=RevokeMsg";
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
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "撤回消息", $"群:{message.groupid} QQ:{{保留参数}} 消息:{message.text}");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "撤回消息", $"调用异常，信息{ret["Msg"]}");
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=QQZan";
            JObject data = new JObject
            {
                {"UserID",qqId}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "发送赞", $"向{qqId}发送了一个赞");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "发送赞", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_getCookiesV2", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getCookiesV2(int authCode, IntPtr domain)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetUserCook";
            JObject data = new JObject();
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "获取cookie", $"已获取个人cookie");
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
            if (!Directory.Exists("data\\app\\" + appid))
                Directory.CreateDirectory("data\\app\\" + appid);
            string path = "data\\app\\" + appid;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            return Marshal.StringToHGlobalAnsi(directoryInfo.FullName + "\\");
        }

        [DllExport(ExportName = "CQ_getLoginQQ", CallingConvention = CallingConvention.StdCall)]
        public static long CQ_getLoginQQ(int authCode)
        {
            return Save.curentQQ;
        }

        [DllExport(ExportName = "CQ_getLoginNick", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getLoginNick(int authCode)
        {
            FriendsList friendsList = JsonConvert.DeserializeObject<FriendsList>(WebAPI.GetFriendList());
            return Marshal.StringToHGlobalAnsi(friendsList.List.Where(x => x.FriendUin == Save.curentQQ).FirstOrDefault().NickName);
        }

        [DllExport(ExportName = "CQ_setGroupKick", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupKick(int authCode, long groupId, long qqId, bool refuses)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GroupMgr";
            JObject data = new JObject
            {
                {"ActionType",3},
                {"GroupID",groupId},
                {"ActionUserID",qqId},
                {"Content","" }
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "群踢人", $"从群{groupId} 踢出了{qqId}");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "群踢人", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupBan", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupBan(int authCode, long groupId, long qqId, long time)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x570_8";
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
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "群禁言", $"在群{groupId} 禁言了{qqId} {time / 60}分钟");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "群禁言", $"调用异常，信息未知");
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x8fc_2";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"UserID",qqId},
                {"NewTitle",title.ToString(GB18030)}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "群头衔发放", $"在群{groupId} 给{qqId} 发放头衔{title}");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "群头衔发放", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupWholeBan", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupWholeBan(int authCode, long groupId, bool isOpen)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=OidbSvc.0x89a_0";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"Switch",isOpen?1:0}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "全员禁言", $"在群{groupId} {(isOpen ? "开启" : "关闭")}了全员禁言");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "全员禁言", $"调用异常，信息未知");
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=ModifyGroupCard";
            JObject data = new JObject
            {
                {"GroupID",groupId},
                {"UserID",qqId},
                {"NewNick",newCard.ToString(GB18030)}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["GroupID"].ToString() != "0" ? 0 : -1);
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "群名片修改", $"在群{groupId} 修改{qqId}的群名片为{newCard.ToString(GB18030)}");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "群名片修改", $"调用异常，信息未知");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupLeave", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupLeave(int authCode, long groupId, bool isDisband)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GroupMgr";
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
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "退群", $"退出了群{groupId}");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "退群", $"调用异常，信息{ret["Msg"]}");
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=DealFriend";
            JToken data = JObject.Parse(request.json)["CurrentPacket"]["Data"]["EventData"];
            data["Action"] = requestType == 1 ? 2 : 3;
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "处理好友请求", $"{(requestType == 1 ? "同意" : "拒绝")}了" +
                    $"好友{Convert.ToInt64(data["UserID"].ToString())}的请求");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "处理好友请求", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_setGroupAddRequestV2", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setGroupAddRequestV2(int authCode, IntPtr identifying, int requestType, int responseType, IntPtr appendMsg)
        {
            Requests request = Requests.First(x => x.type == "GroupRequest");
            Requests.Remove(request);
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=DealFriend";
            JToken data = JObject.Parse(request.json)["CurrentPacket"]["Data"]["EventData"];
            data["Action"] = responseType == 1 ? 11 : 21;
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            int state = Convert.ToInt32(ret["Ret"].ToString());
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            if (state == 0)
                LogHelper.WriteLog(LogLevel.InfoSuccess, pluginname, "处理加群请求", $"{(responseType == 1 ? "同意" : "拒绝")}了" +
                    $"QQ{Convert.ToInt64(data["UserID"].ToString())}" +
                    $"对群{Convert.ToInt64(data["FromGroupId"].ToString())}的请求");
            else
                LogHelper.WriteLog(LogLevel.Error, pluginname, "处理加群请求", $"调用异常，信息{ret["Msg"]}");
            return state;
        }

        [DllExport(ExportName = "CQ_addLog", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_addLog(int authCode, int priority, IntPtr type, IntPtr msg)
        {
            if (authCode == 0)
            {
                LogHelper.WriteLog(priority, "OPQBot框架", type.ToString(GB18030), msg.ToString(GB18030));
            }
            else
            {
                string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
                LogHelper.WriteLog(priority, pluginname, type.ToString(GB18030), msg.ToString(GB18030));
            }
            return 0;
        }

        [DllExport(ExportName = "CQ_setFatal", CallingConvention = CallingConvention.StdCall)]
        public static int CQ_setFatal(int authCode, IntPtr errorMsg)
        {
            string pluginname = appInfos.Find(x => x.AuthCode == authCode).Name;
            LogHelper.WriteLog(LogLevel.Fatal, pluginname, "异常抛出", errorMsg.ToString(GB18030),"");
            //待找到实现方法
            throw new Exception(errorMsg.ToString(GB18030));
        }

        [DllExport(ExportName = "CQ_getGroupMemberInfoV2", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupMemberInfoV2(int authCode, long groupId, long qqId, bool isCache)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=friendlist.GetTroopMemberListReq";
            JObject data = new JObject
            {
                {"GroupUin",groupId},
                {"LastUin",0}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            JArray array = JArray.Parse(ret["MemberList"].ToString());
            JToken targetuser = null;
            foreach (var item in array)
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=friendlist.GetTroopMemberListReq";
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
                binaryWriterMain.Write(stream.ToArray());
            }
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(streamMain.ToArray()));
        }

        [DllExport(ExportName = "CQ_getGroupList", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getGroupList(int authCode)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetGroupList";
            JObject data = new JObject
            {
                {"NextToken",""}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));//获取请求之后的消息
            MemoryStream streamMain = new MemoryStream();
            BinaryWriter binaryWriterMain = new BinaryWriter(streamMain);//最终要进行编码的字节流
            JArray grouplist = JArray.Parse(ret["TroopList"].ToString());
            BinaryWriterExpand.Write_Ex(binaryWriterMain, grouplist.Count);//群数量
            foreach (var item in grouplist)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter binaryWriter = new BinaryWriter(stream);//临时字节流,用于统计每个群信息的字节数量

                BinaryWriterExpand.Write_Ex(binaryWriter, Convert.ToInt64(item["GroupId"].ToString()));
                BinaryWriterExpand.Write_Ex(binaryWriter, item["GroupName"].ToString());

                BinaryWriterExpand.Write_Ex(binaryWriterMain, (short)stream.Length);//将临时字节流的字节长度写入主字节流
                binaryWriterMain.Write(stream.ToArray());//写入数据
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
            string filename = file.ToString(GB18030);
            string path = $"data\\image\\{filename}.cqimg";
            IniConfig ini = new IniConfig(path);
            ini.Object.Add(new ISection("image"));
            string picurl = ini.Object["image"]["url"];
            if (!File.Exists($"data\\image\\" + filename + ".cqimg"))
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
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetGroupList";
            JObject data = new JObject
            {
                {"NextToken",""}
            };
            JObject ret = JsonConvert.DeserializeObject<JObject>(SendRequest(url, data.ToString()));
            JArray array = JArray.Parse(ret["TroopList"].ToString());
            JToken targetgroup = null;
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
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(stream.ToArray()));
        }

        [DllExport(ExportName = "CQ_getFriendList", CallingConvention = CallingConvention.StdCall)]
        public static IntPtr CQ_getFriendList(int authCode, bool reserved)
        {
            string url = $@"{Save.url}v1/LuaApiCaller?qq={Save.curentQQ}&funcname=GetQQUserList";
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
                binaryWriterMain.Write(stream.ToArray());
            }
            return Marshal.StringToHGlobalAnsi(Convert.ToBase64String(streamMain.ToArray()));
        }
        [DllExport(ExportName = "cq_start", CallingConvention = CallingConvention.StdCall)]
        public static bool cq_start(IntPtr path, int authcode)
        {
            string pathtext = path.ToString(GB18030);
            JsonLoadSettings loadsetting = new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            };
            JObject jObject = JObject.Parse(File.ReadAllText(pathtext.Replace(".dll", ".json")), loadsetting);
            DllHelper dllHelper = new DllHelper(pathtext);
            KeyValuePair<int, string> appinfotext = dllHelper.GetAppInfo();
            AppInfo appInfo = new AppInfo(appinfotext.Value, 0, appinfotext.Key
                , jObject["name"].ToString(), jObject["version"].ToString(), Convert.ToInt32(jObject["version_id"].ToString())
                , jObject["author"].ToString(), jObject["description"].ToString(), authcode);
            appInfos.Add(appInfo);
            return true;
        }
        public static string SendRequest(string url, string data)
        {
            return WebAPI.SendRequest(url, data);
        }
    }
}
