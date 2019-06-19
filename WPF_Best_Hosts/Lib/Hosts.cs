using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using WPF_Best_Hosts.Model;

namespace WPF_Best_Hosts.Lib
{
    public static class Hosts
    {

        public static string HostsPath = @"C:\WINDOWS\system32\drivers\etc\hosts";
        private static Encoding _encoding;
        public static ObservableCollection<HostsData> hostsDatas = new ObservableCollection<HostsData>();
        public static void updateHosts(string data)
        {
            //通常情况下这个文件是只读的，所以写入之前要取消只读
            var fileList = ReadHosts();

            var fileListData = fileList.FirstOrDefault(u => Regex.IsMatch(u, $" {data.Split(' ').Last()}"));
            if (!(fileListData is null))
                fileList.Remove(fileListData);

            fileList.Add(data);


            using (var file = File.OpenWrite(HostsPath))
            using (var stream = new StreamWriter(file, Encoding))
            {
                foreach (var line in fileList) stream.WriteLine(line);
            }
        }

        public static List<string> ReadHosts()
        {
            var encode = Encoding;
            var fileList = new List<string>();
            using (var file = File.OpenRead(HostsPath))
            using (var stream = new StreamReader(file, encode))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        fileList.Add(line);
                }
            }

            return fileList;
        }

        private static Encoding Encoding
        {
            get
            {
                if (_encoding is null)
                {
                    try
                    {
                        File.SetAttributes(HostsPath, File.GetAttributes(HostsPath) & ~FileAttributes.ReadOnly); //取消只读
                        using (var fs = new FileStream(HostsPath, FileMode.Open, FileAccess.Read))
                            _encoding = GetType(fs);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("权限不足，请使用管理员权限打开");
                        throw;
                    }
                    
                }
                return _encoding;
            }
        }

        public static void InitHostsData()
        {
            var hostsLines = Hosts.ReadHosts();
            hostsDatas.Clear();
            foreach (var line in hostsLines)
            {
                var lineTrim = line.Trim();
                if (Regex.IsMatch(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+[ \\t]{1,}[\\.\\w]+$"))
                {
                    hostsDatas.Add(new HostsData()
                    {
                        Ip = Regex.Match(lineTrim, "\\d+\\.\\d+\\.\\d+\\.\\d+").Value,
                        Domain = lineTrim.Split(' ').Last(),
                        State = line.StartsWith("#") ? "失效" : "激活"
                    });
                }
            }
        }

        /// <summary>
        ///     通过给定的文件流，判断文件的编码类型
        /// </summary>
        /// <param name=“fs“>文件流
        /// 
        /// 
        /// </param>
        /// <returns>文件的编码类型</returns>
        private static Encoding GetType(FileStream fs)
        {
            byte[] Unicode = {0xFF, 0xFE, 0x41};
            byte[] UnicodeBIG = {0xFE, 0xFF, 0x00};
            byte[] UTF8 = {0xEF, 0xBB, 0xBF}; //带BOM 
            var reVal = Encoding.Default;

            var r = new BinaryReader(fs, Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            var ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                reVal = Encoding.UTF8;
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
                reVal = Encoding.BigEndianUnicode;
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41) reVal = Encoding.Unicode;
            r.Close();
            return reVal;
        }

        /// <summary>
        ///     判断是否是不带 BOM 的 UTF8 格式
        /// </summary>
        /// <param name=“data“>
        /// 
        /// 
        /// </param>
        /// <returns></returns>
        private static bool IsUTF8Bytes(byte[] data)
        {
            var charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (var i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0) charByteCounter++;
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6) return false;
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80) return false;
                    charByteCounter--;
                }
            }

            if (charByteCounter > 1) throw new Exception("非预期的byte格式");
            return true;
        }
    }
}