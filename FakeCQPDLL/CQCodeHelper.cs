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
            bool Picflag = false, Voiceflag = false;
            //List<long> atQQs = new List<long>();
            foreach (var item in cqCodeList)
            {
                switch (item.Function)
                {
                    case CQFunction.At://[CQ:at,qq=xxxx]
                        {
                            if (!data.ContainsKey("Content")) data.Add("Content", "");
                            text = text.Replace(item.ToSendString(), $"[ATUSER({Convert.ToInt64(item.Items["qq"])})]");
                            //atQQs.Add(Convert.ToInt64(item.Items["qq"]));
                            //Atflag = true;
                            break;
                        }
                    case CQFunction.Image:
                        {
                            if (!data.ContainsKey("Content")) data.Add("Content", "");
                            if (!data.ContainsKey("PicPath")) data.Add("PicPath", "");
                            if (item.Items.ContainsKey("url"))
                                data["PicUrl"] = item.Items["url"];
                            else if (item.Items.ContainsKey("file"))
                            {
                                if (File.Exists("data\\image\\" + item.Items["file"] + ".cqimg"))
                                {
                                    IniConfig ini = new IniConfig("data\\image\\" + item.Items["file"] + ".cqimg"); ini.Load();
                                    data["PicUrl"] = ini.Object["image"]["url"].ToString();
                                    Picflag = true;
                                    break;
                                }
                                string path = item.Items["file"];
                                if (File.Exists("data\\image\\" + path))
                                {
                                    string PicFullPath = new FileInfo("data\\image\\" + path).FullName;
                                    data["PicPath"] = $"/mnt/{WindowsPath2LinuxPath(PicFullPath)}";
                                }
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
                            if (!data.ContainsKey("Content")) data.Add("Content", "");
                            if (item.Items.ContainsKey("file"))
                            {
                                if (!data.ContainsKey("VoicePath")) data.Add("VoicePath", "");
                                string voicepath = $"data\\record\\{item.Items["file"]}";
                                if (File.Exists(voicepath))
                                {
                                    try
                                    {
                                        if (!data.ContainsKey("VoiceUrl")) data.Add("VoiceUrl", "");
                                        IniConfig ini = new IniConfig(voicepath); ini.Load();
                                        data["VoiceUrl"] = ini.Object["record"]["url"].ToString();
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
                                        data["VoicePath"] = $"/mnt/{WindowsPath2LinuxPath(voicepath)}";
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
            if (!string.IsNullOrEmpty(filtered)) data["Content"] = filtered;
            if (Picflag)
                data.Add("SendMsgType", "PicMsg");
            else if (Voiceflag)
                data.Add("SendMsgType", "VoiceMsg");
            else
                data.Add("SendMsgType", "TextMsg");
        }

        private static bool SilkEncode(string voicepath, string extension)
        {
            if (!Directory.Exists("tools"))
            {
                CoreHelper.LogWriter(Save.logListView, (int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools目录丢失，无法继续");
                return false;
            }
            if (!File.Exists(@"tools\silk_v3_encoder.exe"))
            {
                CoreHelper.LogWriter(Save.logListView, (int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools\\silk_v3_encoder.exe 文件丢失，无法继续");
                return false;
            }
            if (!File.Exists(@"tools\ffmpeg.exe"))
            {
                CoreHelper.LogWriter(Save.logListView, (int)CQLogLevel.Error, "音频格式转换", "工具丢失", "...", "tools\\ffmpeg.exe 文件丢失，无法继续");
                return false;
            }
            string output;
            RunCMDCommand($"tools\\ffmpeg.exe -y -i \"{voicepath}\" -f s16le -ar 24000 -ac 1 \"{voicepath.Replace(extension, ".pcm")}\"", out output);
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");
            string filePath = "logs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_pcm.log";
            IniConfig ini = new IniConfig(filePath);
            ini.Object.Add(new Native.Tool.IniConfig.Linq.ISection("OutPut"));
            ini.Object["OutPut"]["Details"] = new Native.Tool.IniConfig.Linq.IValue(output);
            ini.Save();
            if (output.Contains("Invalid data found when processing input"))
            {
                CoreHelper.LogWriter(Save.logListView, (int)CQLogLevel.Error, "音频格式转换", "格式错误", "...", "接受的音频可能不是FFmpeg可转换的格式");
                return false;
            }
            if (!output.Contains("video:0kB"))
            {
                CoreHelper.LogWriter(Save.logListView, (int)CQLogLevel.Error, "音频格式转换", "未知错误", "...", $"FFmpeg输出已保存至{filePath}");
                return false;
            }
            string filepath = voicepath.Replace(extension, ".pcm");
            RunCMDCommand($"tools\\silk_v3_encoder.exe \"{filepath}\" \"{filepath.Replace(".pcm", ".silk")}\" -tencent -quiet", out output);
            filePath = "logs\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_silk.log";
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
