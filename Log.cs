using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Log
{
    static string DirPath = Environment.CurrentDirectory + @"\Log";
   
    static string temp;

    public void LogWrite(string str)
    {
        string FilePath = DirPath + "\\Log_" + DateTime.Today.ToString("yyyyMMdd") + ".log";
        DirectoryInfo directoryInfo = new DirectoryInfo(DirPath);
        FileInfo fileInfo = new FileInfo(FilePath);
        try
        {
            if (!directoryInfo.Exists) Directory.CreateDirectory(DirPath);
            if (!fileInfo.Exists)
            {
                using (StreamWriter sw = new StreamWriter(FilePath))
                {
                    temp = string.Format("[{0}] {1}", DateTime.Now, str);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(FilePath))
                {
                    temp = string.Format("[{0}] {1}", DateTime.Now, str);
                    sw.WriteLine(temp);
                    sw.Close();
                }
            }
        }
        catch (Exception e)
        {
        }

    }

}