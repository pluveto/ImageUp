using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace ImageUpWpf.Util
{
    public static class FileUtil
    {

        public static Stream BitmapToJpegStream(BitmapSource src)
        {            
            Stream s = new MemoryStream();
            var enc = new JpegBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(new CachedBitmap(src, BitmapCreateOptions.None, BitmapCacheOption.OnLoad)));
            enc.Save(s);
            return s;
        }

        public static void SaveStream(string fileFullPath, Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            FileStream fileStream = File.Create(fileFullPath, (int)stream.Length);
            byte[] buffer = new byte[stream.Length];
            // 将 stream 暂存到 buffer
            stream.Read(buffer, 0, buffer.Length);
            // Use write method to write to the file specified above
            fileStream.Write(buffer, 0, buffer.Length);
            //Close the filestream
            fileStream.Close();
        }
    }
}
