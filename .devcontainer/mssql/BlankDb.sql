/*
    This will generate a blank database when the container is spun up
    that you can use to connect to for the SQL configuration in the web installer flow

    ---- NOTE ----
    Any .sql files in this folder will be executed 
    Along with any .dacpac in the folder will be restored as databases 
    See postCreateCommand.sh for specifics 
*/
CREATE DATABASE UmbracoUnicore;
GO