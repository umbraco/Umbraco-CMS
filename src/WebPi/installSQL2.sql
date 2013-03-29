/**********************************************************************/
/* CreateUser.SQL */
/* Creates a user and makes the user a member of db roles */
/* This script runs against the User database and requires connection string */
/* Supports SQL Server and SQL AZURE */
/**********************************************************************/

-- Create database user and map to login
-- and add user to the datareader, datawriter, ddladmin and securityadmin roles
--

CREATE USER PlaceHolderForUser FOR LOGIN PlaceHolderForUser;
GO
EXEC sp_addrolemember 'db_owner', PlaceHolderForUser;
GO
