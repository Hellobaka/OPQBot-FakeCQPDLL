using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CQP
{
    public class DllHelper
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String path);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr lib, String funcName);

        [DllImport("kernel32.dll")]
        private extern static int GetLastError();

        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr lib);

        private IntPtr hLib;

        public DllHelper(string filepath)
        {
            if (hLib == null || hLib == (IntPtr)0)
            {
                try { hLib = LoadLibrary(filepath); }
                catch
                {
                    Console.WriteLine($"插件 {filepath} 载入失败,LoadLibrary :GetLastError={GetLastError()}");
                }
            }
            if (hLib != (IntPtr)0)
            {
                Initialize = (Type_Initialize)Invoke("Initialize", typeof(Type_Initialize));
                AppInfo = (Type_AppInfo)Invoke("AppInfo", typeof(Type_AppInfo));
            }
        }

        ~DllHelper()
        {
            if (hLib != null)
                FreeLibrary(hLib);
        }

        //将要执行的函数转换为委托
        public Delegate Invoke(String APIName, Type t)
        {
            IntPtr api = GetProcAddress(hLib, APIName);
            return (Delegate)Marshal.GetDelegateForFunctionPointer(api, t);
        }

        private delegate IntPtr Type_AppInfo();
        private delegate IntPtr Type_Initialize(int authcode);
        private static Type_AppInfo AppInfo;
        private static Type_Initialize Initialize;

        //调用库中事件
        public int DoInitialize(int authcode)
        {
            Initialize(authcode);
            return 0;
        }

        public KeyValuePair<int, string> GetAppInfo()
        {
            string appinfo = Marshal.PtrToStringAnsi(AppInfo());
            string[] pair = appinfo.Split(',');
            if (pair.Length != 2)
                throw new Exception("获取AppInfo信息失败");
            KeyValuePair<int, string> valuePair = new KeyValuePair<int, string>(Convert.ToInt32(pair[0]), pair[1]);
            return valuePair;
        }
    }
}
