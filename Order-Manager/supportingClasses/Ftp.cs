using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Order_Manager.supportingClasses
{
    public class Ftp
    {
        // fields for credentials
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

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
            // create an FTP Request
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(Host + "/" + remoteFile);

            // log in to the FTP Server with the username and password provided
            ftpRequest.Credentials = new NetworkCredential(Username, Password);

            // When in doubt, use these options
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = true;

            // specify the Type of FTP request
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            // establish return communication with the FTP Server
            FtpWebResponse ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();

            // get the server's response stream
            Stream ftpStream = ftpResponse.GetResponseStream();

            // open file stream to download file that has been downloaded 
            FileStream localFileStream = new FileStream(localFile, FileMode.Create);

            // fields for data download
            byte[] byteBuffer = new byte[2048];
            int bytesRead = ftpStream.Read(byteBuffer, 0, 2048);

            // start downloading
            while (bytesRead > 0)
            {
                localFileStream.Write(byteBuffer, 0, bytesRead);
                bytesRead = ftpStream.Read(byteBuffer, 0, 2048);
            }

            // ending job
            localFileStream.Close();
            ftpStream.Close();
            ftpResponse.Close();
            ftpRequest = null;
        }
    }
}
