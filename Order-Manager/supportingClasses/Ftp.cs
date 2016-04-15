using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Order_Manager.supportingClasses
{
    /*
     * A class for FTP server connection
     */
    public class Ftp
    {
        // fields for credentials
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        /* constructor that initialize ftp connection credentials */
        public Ftp(string host, string username, string password)
        {
            Host = host;
            Username = username;
            Password = password;
        }

        /* a method that return all the file name on the directory provided */
        public string[] GetFileList(string directory)
        {
            // get the object used to communicate with the server
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Host + '/' + directory);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            // declare credentials
            request.Credentials = new NetworkCredential(Username, Password);

            // get response from the server
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            // adding all the files on the directory
            List<string> list = new List<string>();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string line = reader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                list.Add(line);
                line = reader.ReadLine();
            }

            reader.Close();
            response.Close();

            return list.ToArray();
        }

        /* a method that download file on the ftp server from the path provided */
        public void Download(string remoteFile, string localFile)
        {
            // get the object used to communicate with the server
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Host + '/' + remoteFile);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            // declare credentials
            request.Credentials = new NetworkCredential(Username, Password);

            // get response from the server
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            // save file
            StreamWriter writer = new StreamWriter(localFile);
            writer.WriteLine(reader.ReadToEnd());

            writer.Close();
            reader.Close();
            response.Close();
        }

        /* a method that delete file on the ftp server from the path provided */
        public bool Delete(string remoteFile)
        {
            try
            {
                // get the object used to communicate with the server
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Host + '/' + remoteFile);
                request.Method = WebRequestMethods.Ftp.DeleteFile;

                // declare credentials
                request.Credentials = new NetworkCredential(Username, Password);

                // get response from the server
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                response.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
