using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;
using SpiderCore;

namespace Spider
{
    // TODO: Should be moved to FileHandler?    
    public static class StringCompressor
    {
        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Compress to a file.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CreateZipFile(string text, int temp)
        {
            string fileName = @Directory.GetCurrentDirectory() + @"\spiderData.txt";
            string error;

            try
            {
                // Delete the file if it exists. 
                if (File.Exists(fileName))
                    File.Delete(fileName);

                // Create the file. 
                using (FileStream fs = File.Create(@fileName))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(text);
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex) { error = ex.Message; }

            string zipFile = @Directory.GetCurrentDirectory() + @"spiderData.zip";

            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(@fileName, "");
                zip.Save(@zipFile);
            }

            return zipFile;
        }

        /// <summary>
        /// Compress to file
        /// </summary>
        /// <param name="jsonFileName"></param>
        /// <returns></returns>
        public static string CreateZipFile(string jsonFileName)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(jsonFileName);
                zip.Save(String.Format("{0}.zip", @jsonFileName));
            }

            return String.Format("{0}.zip", @jsonFileName);
        }

        /// <summary>
        /// Compress to file
        /// </summary>
        /// <param name="jsonFileName"></param>
        /// <param name="metaFileName"></param>
        /// <returns></returns>
        public static string CreateZipFile(string jsonFileName, string metaFileName)
        {
            try
            {
                using (ZipFile zip = new ZipFile())
                {
                    zip.AddFile(@jsonFileName, "");
                    zip.AddFile(@metaFileName, "");
                    zip.Save(String.Format("{0}.zip", @jsonFileName));
                }

                return String.Format("{0}.zip", @jsonFileName);
            }
            catch(Exception ex)
            {
                string em = ex.Message;
                return null;
            }
        }            
      
        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
