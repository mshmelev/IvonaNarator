using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace IvonaNarator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("program.exe file.txt");
                return;
            }

            var api = new IvonaApi(
                ConfigurationManager.AppSettings["IvonaAccessKey"],
                ConfigurationManager.AppSettings["IvonaSecretKey"],
                "Tatyana", "ru-RU", "Female");
                //"Maxim", "ru-RU", "Male");

            try
            {
                const int maxPartSize = 8192;
                var paragraphs = File.ReadAllText(args[0]).Replace("\r", "").Split('\n');

                int outFiles = 0;
                for (int i = 0; i < paragraphs.Length;)
                {
                    var text = paragraphs[i];
                    while (i < paragraphs.Length-1 && text.Length + paragraphs[i + 1].Length + 1 <= maxPartSize)
                    {
                        text += '\n' + paragraphs[i + 1];
                        ++i;
                    }

                    byte[] audioData= api.CreateSpeech(text);
                    var path = outFiles.ToString("D4")+".mp3";
                    ++outFiles;
                    File.WriteAllBytes(path, audioData);
                    Console.WriteLine(path);

                    ++i;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
                foreach (string header in ex.Response.Headers)
                    Console.WriteLine("{0}: {1}", header, ex.Response.Headers[header]);

                using (var responseStream = ex.Response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var streamReader = new StreamReader(responseStream))
                            Console.WriteLine(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
