using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeepSeekDeskBand
{
    /// <summary>
    /// Windows Credential Manager 封装
    /// 安全存储 API Key，使用 Windows 内置加密存储
    /// </summary>
    public static class CredentialManager
    {
        private const string TargetName = "DeepSeekDeskBand:ApiKey";
        private const int CRED_TYPE_GENERIC = 1;
        private const int CRED_PERSIST_LOCAL_MACHINE = 2;

        #region P/Invoke

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UserName;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredWriteW(ref CREDENTIAL credential, int flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredReadW(string targetName, int type, int flags, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredDeleteW(string targetName, int type, int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr buffer);

        #endregion

        /// <summary>
        /// 保存 API Key 到 Credential Manager
        /// </summary>
        public static void SaveApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                DeleteApiKey();
                return;
            }

            byte[] blob = Encoding.Unicode.GetBytes(apiKey);
            var credential = new CREDENTIAL
            {
                Type = CRED_TYPE_GENERIC,
                TargetName = TargetName,
                CredentialBlobSize = blob.Length,
                CredentialBlob = Marshal.AllocHGlobal(blob.Length),
                Persist = CRED_PERSIST_LOCAL_MACHINE,
                UserName = Environment.UserName
            };

            try
            {
                Marshal.Copy(blob, 0, credential.CredentialBlob, blob.Length);
                if (!CredWriteW(ref credential, 0))
                {
                    throw new InvalidOperationException(
                        $"CredWrite 失败，错误码: {Marshal.GetLastWin32Error()}");
                }
            }
            finally
            {
                if (credential.CredentialBlob != IntPtr.Zero)
                    Marshal.FreeHGlobal(credential.CredentialBlob);
            }
        }

        /// <summary>
        /// 从 Credential Manager 读取 API Key
        /// 返回 null 表示未设置
        /// </summary>
        public static string? LoadApiKey()
        {
            IntPtr credPtr;
            if (!CredReadW(TargetName, CRED_TYPE_GENERIC, 0, out credPtr))
                return null;

            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                if (credential.CredentialBlobSize <= 0)
                    return null;

                byte[] blob = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, blob, 0, blob.Length);
                return Encoding.Unicode.GetString(blob).TrimEnd('\0');
            }
            finally
            {
                CredFree(credPtr);
            }
        }

        /// <summary>
        /// 删除已保存的 API Key
        /// </summary>
        public static bool DeleteApiKey()
        {
            return CredDeleteW(TargetName, CRED_TYPE_GENERIC, 0);
        }

        /// <summary>
        /// 检查 API Key 是否已保存
        /// </summary>
        public static bool HasApiKey()
        {
            return LoadApiKey() != null;
        }
    }
}
