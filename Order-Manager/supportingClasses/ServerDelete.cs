using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace Order_Manager.supportingClasses
{
    /*
     * A static class that delete file on the server
     */
    public static class ServerDelete
    {
        /* the only function in the class that delete file on the sftp server */
        public static void Delete(string host, string username, string password, string fileName)
        {
            JSch js = new JSch();

            // declare sftp connection 
            Sftp sftp = new Sftp(host, username, password);
            sftp.Connect();

            // Create a session with SFTP credentials
            Session session = js.getSession(sftp.Username, sftp.Host);

            // Get a UserInfo object
            UserInfo ui = new UInfo(sftp.Password);

            // Pass user info to session
            session.setUserInfo(ui);

            // Open the session
            session.connect();

            // Tell it is an SFTP
            Channel channel = session.openChannel("sftp");
            ChannelSftp cSftp = (ChannelSftp)channel;
            cSftp.connect();

            // Delete the file
            cSftp.rm(fileName);

            // disconnection
            channel.disconnect();
            cSftp.exit();
            sftp.Close();
        }

        /* a supportin class for deleting file */
        private class UInfo : UserInfo
        {
            private readonly string passwd = string.Empty;

            public UInfo(string pwd) { passwd = pwd; }

            public string getPassword() { return passwd; }

            #region Dummy Implementations
            public bool promptYesNo(string str) { return true; }

            public string getPassphrase() { return null; }

            public bool promptPassphrase(string message) { return true; }

            public bool promptPassword(string message) { return true; }

            public void showMessage(string message) { }
            #endregion Dummy Implementations
        }
    }
}
