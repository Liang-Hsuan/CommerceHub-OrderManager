using Tamir.SharpSsh;
using Tamir.SharpSsh.jsch;

namespace CommerceHub_OrderManager.supportingClasses
{
    /*
     * A static class that delete file on the server
     */
    public static class ServerDelete
    {
        /* the only function in the class that delete file on the sftp server */
        public static void delete(Sftp sftp, string fileName)
        {
            JSch js = new JSch();

            // Create a session with SFTP credentials
            sftp.Connect();
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
            string passwd = string.Empty;

            public UInfo() { passwd = string.Empty; }

            public UInfo(string pwd) { passwd = pwd; }

            public string getPassword() { return passwd; }

            public string Password
            {
                set { passwd = value; }

                get { return passwd; }
            }

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
