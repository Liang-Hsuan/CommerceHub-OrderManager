# Order Manager #
## A desktop application that can manage various shopping channels' orders ##

**The main function of this application:**

*  the program will check new order from shopping channels' servers and download them to process, then update the data to database.

* you can deal with the new orders in batch, which will process the orders all in default settings (default shipment service for the channel, package type for the item, no item cancellation, etc), or you can do it in a detail page to specify any needs for the order. 

* a convenient program that allow user to manage orders from different online shopping channels in one single place.

-------------

**The safty of this application:**

*  the production version of the application will need to log in which accept the credentials for database connection, so the connection string will only be complete if the user type in the correct username and password.

*  since all credentials are grabbed from database and even database connection string requires user input, there is no confidential data anywhere in the code.

-------------

**Third party libraries**

* iTextSharp (for packing slip generation) & TamirSSH (for SFTP server connection) & Ftp (my own open source class for FTP server connection)

-------------

**Note:**
This application is a very flexible and modularized in order to handle more than one shopping channels' order, so adding a new shopping channel is pretty easy, and the code is very well documented.
