using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wpf_github_hosts
{
    public class Hosts
    {
        public static void updateHosts(string data)
        {
            var path = @"C:\WINDOWS\system32\drivers\etc\hosts";
            //通常情况下这个文件是只读的，所以写入之前要取消只读
            File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.ReadOnly); //取消只读
            var fileList = new List<string>();
            var encode = Encoding.UTF8;
            using (var file = File.OpenRead(path))
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                encode = GetType(fs);
                using (var stream = new StreamReader(file, encode))
                {
                    while (!stream.EndOfStream)
                    {
                        var line = stream.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                            fileList.Add(line);
                    }
                }
            }

            var fileListData = fileList.FirstOrDefault(u => Regex.IsMatch(u, $" {data.Split(' ').Last()}"));
            if (!(fileListData is null))
                fileList.Remove(fileListData);

            fileList.Add(data);


            using (var file = File.OpenWrite(path))
            using (var stream = new StreamWriter(file, encode))
            {
                foreach (var line in fileList) stream.WriteLine(line);
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