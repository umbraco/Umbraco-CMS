/*******************************************************************************************

    Umbraco database installation script for SQL Server (upgrade from Umbraco 4.0.x)
 
	IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.8.0.0
    
    Please increment this version number if ANY change is made to this script,
    so compatibility with scripts for other database systems can be verified easily.
    The first 3 digits depict the Umbraco version, the last digit is the database version.
    (e.g. version 4.0.0.3 means "Umbraco version 4.0.0, database version 3")
    
    Check-in policy: only commit this script if
     * you ran the Umbraco installer completely;
     * you ran it on the targetted database system;
     * you ran the Runway and Module installations;
     * you were able to browse the Boost site;
     * you were able to open the Umbraco administration panel;
     * you have documented the code change in this script;
     * you have incremented the version number in this script.
 
	IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT

********************************************************************************************/

/* REMOVE CONSTRAINTS */
ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoApp] 
;
ALTER TABLE [umbracoAppTree] DROP CONSTRAINT [FK_umbracoAppTree_umbracoApp] 
;