using Native.Tool.IniConfig;
using Newtonsoft.Json.Linq;
using Launcher.Sdk.Cqp.Enum;
using Launcher.Sdk.Cqp.Expand;
using Launcher.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Deserizition;

namespace CQP
{
    public static class CQCodeHelper
    {
        public static void Progeress(List<CQCode> cqCodeList, ref JObject data, ref string text)
        {
            bool Picflag = text.Contains("[CQ:image"), Voiceflag = text.Contains("[CQ:record");
            string contentSectionName = "Content";
            if (Picflag || Voiceflag)
                contentSectionName = "content";
            foreach (var item in cqCodeList)
            {
                switch (item.Function)
                {
                    case CQFunction.At://[CQ:at,qq=xxxx]
                        {
                            if (!data.ContainsKey(contentSectionName)) data.Add(contentSectionName, "");
                            text = text.Replace(item.ToSendString(), $"[ATUSER({Convert.ToInt64(item.Items["qq"])})]");
                            break;
                        }
                    case CQFunction.Image:
                        {
                            if (!data.ContainsKey("content")) data.Add("content", "");
                            if (!data.ContainsKey("picBase64Buf")) data.Add("picBase64Buf", "");
                            if (!data.ContainsKey("picUrl")) data.Add("picUrl", "");
                            if (!data.ContainsKey("atUser")) data.Add("atUser", 0);
                            if (!data.ContainsKey("fileMd5")) data.Add("fileMd5", "");
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
                            text = Regex.Replace(text, @"\[CQ:image,.*?\]", "[PICFLAG]");
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
                                        IniConfig ini = new IniConfig(voicepath); ini.Load();
                                        data["voiceUrl"] = ini.Object["record"]["url"].ToString();
                                    }
                                    catch
                                    {
                                        string extension = new FileInfo(voicepath).Extension;
                                        if (extension != ".silk")
                                        {
                                            if (SilkEncode(voicepath, extension))
                                                voicepath = voicepath.Replace(extension, ".silk");
                                        }
                                        voicepath = new FileInfo(voicepath).FullName;
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
                            int src = Convert.ToInt32(item.Items["id"]);
                            text = text.Replace(item.ToSendString(), Encoding.UTF32.GetString(BitConverter.GetBytes(src)));
                            break;
                        }
                }
            }
            string filtered = Regex.Replace(text, @"\[CQ.*\]", "");
            if (!string.IsNullOrEmpty(filtered)) data[contentSectionName] = filtered;
            if (Picflag)
                data.Add("sendMsgType", "PicMsg");
            else if (Voiceflag)
                data.Add("sendMsgType", "VoiceMsg");
            else
                data.Add("SendMsgType", "TextMsg");
        }

        private static bool SilkEncode(string voicepath, string extension)
        {
            if (!Directory.Exists("tools"))
            {
                LogHelper.WriteLog((int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools目录丢失，无法继续");
                return false;
            }
            if (!File.Exists(@"tools\silk_v3_encoder.exe"))
            {
                LogHelper.WriteLog((int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools\\silk_v3_encoder.exe 文件丢失，无法继续");
                return false;
            }
            if (!File.Exists(@"tools\ffmpeg.exe"))
            {
                LogHelper.WriteLog((int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools\\ffmpeg.exe 文件丢失，无法继续");
                return false;
            }
            string output;
            RunCMDCommand($"tools\\ffmpeg.exe -y -i \"{voicepath}\" -f s16le -ar 24000 -ac 1 \"{voicepath.Replace(extension, ".pcm")}\"", out output);
            if (!Directory.Exists("logs\\audiologs"))
                Directory.CreateDirectory("logs\\audiologs");
            string filePath = "logs\\audiologs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_pcm.log";
            IniConfig ini = new IniConfig(filePath);
            ini.Object.Add(new Native.Tool.IniConfig.Linq.ISection("OutPut"));
            ini.Object["OutPut"]["Details"] = new Native.Tool.IniConfig.Linq.IValue(output);
            ini.Save();
            if (output.Contains("Invalid data found when processing input"))
            {
                LogHelper.WriteLog((int)CQLogLevel.Error, "音频格式转换", "格式错误", "...", "接受的音频可能不是FFmpeg可转换的格式");
                return false;
            }
            if (!output.Contains("video:0kB"))
            {
                LogHelper.WriteLog((int)CQLogLevel.Error, "音频格式转换", "未知错误", "...", $"FFmpeg输出已保存至{filePath}");
                return false;
            }
            string filepath = voicepath.Replace(extension, ".pcm");
            RunCMDCommand($"tools\\silk_v3_encoder.exe \"{filepath}\" \"{filepath.Replace(".pcm", ".silk")}\" -tencent -quiet", out output);
            filePath = "logs\\audiologs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_silk.log";
            ini = new IniConfig(filePath);
            ini.Object.Add(new Native.Tool.IniConfig.Linq.ISection("OutPut"));
            ini.Object["OutPut"]["Details"] = new Native.Tool.IniConfig.Linq.IValue(output);
            ini.Save();
            return true;
        }
        private static readonly string CMDPath = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\cmd.exe";
        private static void RunCMDCommand(string Command, out string OutPut)
        {
            using (Process pc = new Process())
            {
                Command = Command.Trim().TrimEnd('&') + "&exit";

                pc.StartInfo.FileName = CMDPath;
                pc.StartInfo.CreateNoWindow = true;
                pc.StartInfo.RedirectStandardError = true;
                pc.StartInfo.RedirectStandardInput = true;
                pc.StartInfo.RedirectStandardOutput = true;
                pc.StartInfo.UseShellExecute = false;
                pc.Start();

                pc.StandardInput.WriteLine(Command);
                pc.StandardInput.AutoFlush = true;
                string a = pc.StandardOutput.ReadToEnd();
                string b = pc.StandardError.ReadToEnd();
                OutPut = a + b;
                int P = OutPut.IndexOf(Command) + Command.Length;
                OutPut = OutPut.Substring(P, OutPut.Length - P - 3);
                pc.WaitForExit();
                pc.Close();
            }
        }
        private static string WindowsPath2LinuxPath(string path)
        {
            path = path.Replace(":\\", "/").Replace("\\", "/").ToLower();
            return path;
        }
    }
}
