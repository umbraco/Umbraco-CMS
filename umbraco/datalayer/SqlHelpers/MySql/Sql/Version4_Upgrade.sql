/*******************************************************************************************

    Umbraco database installation script for SQL Server (upgrade from Umbraco 4.0.x)
 
	IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.1.0.2
    
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

/* INSERT NEW MEDIA RECYCLE BIN NODE */
/* remove auto increment so we can insert identity */
ALTER TABLE umbraconode MODIFY COLUMN id INTEGER NOT NULL; 
INSERT INTO umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType, createDate) 
VALUES (-21, 0, -1, 0, 0, '-1,-21', 0, 'BF7C7CBC-952F-4518-97A2-69E9C7B33842', 'Recycle Bin', 'CF3D8E34-1C1C-41e9-AE56-878B57B32113', '2009/08/28 00:28:28.920')
;
/* re-add auto increment */
ALTER TABLE umbraconode MODIFY COLUMN id INTEGER NOT NULL AUTO_INCREMENT; 
/* Add the mediaRecycleBin tree type */
INSERT IGNORE INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'media', 'mediaRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.MediaRecycleBin')
;

CREATE TABLE cmsPreviewXml(
	nodeId int NOT NULL,
	versionId CHAR(36) NOT NULL,
	timestamp datetime NOT NULL,
	xml LONGTEXT NOT NULL)
;
ALTER TABLE cmsPreviewXml ADD CONSTRAINT PK_cmsContentPreviewXml PRIMARY KEY CLUSTERED (nodeId, versionId) 
; 

