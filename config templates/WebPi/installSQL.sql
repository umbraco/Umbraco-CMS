/**********************************************************************/
/* Install.SQL                                                        */
/* Creates a login and makes the user a member of db roles            */
/*                                                                    */
/**********************************************************************/

-- Declare variables for database name, username and password
DECLARE @dbName sysname,
      @dbUser sysname,
      @dbPwd nvarchar(max);

-- Set variables for database name, username and password
SET @dbName = 'PlaceHolderForDb';
SET @dbUser = 'PlaceHolderForUser';
SET @dbPwd = 'PlaceHolderForPassword';

DECLARE @cmd nvarchar(max)

-- Create login
IF( SUSER_SID(@dbUser) is null )
BEGIN
    print '-- Creating login '
    SET @cmd = N'CREATE LOGIN ' + quotename(@dbUser) + N' WITH PASSWORD ='''+ replace(@dbPwd, '''', '''''') + N''''
    EXEC(@cmd)
END

-- Create database user and map to login
-- and add user to the datareader, datawriter, ddladmin and securityadmin roles
--
SET @cmd = N'USE ' + quotename(@DBName) + N'; 
IF( NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + replace(@dbUser, '''', '''''') + N'''))
BEGIN
    print ''-- Creating user'';
    CREATE USER ' + quotename(@dbUser) + N' FOR LOGIN ' + quotename(@dbUser) + N';
    print ''-- Adding user'';
    EXEC sp_addrolemember ''db_ddladmin'', ''' + replace(@dbUser, '''', '''''') + N''';
    print ''-- Adding user'';
    EXEC sp_addrolemember ''db_securityadmin'', ''' + replace(@dbUser, '''', '''''') + N''';
    print ''-- Adding user'';
    EXEC sp_addrolemember ''db_datareader'', ''' + replace(@dbUser, '''', '''''') + N''';
    print ''-- Adding user'';
    EXEC sp_addrolemember ''db_datawriter'', ''' + replace(@dbUser, '''', '''''') + N''';
END'
EXEC(@cmd)
GO