/*******************************************************************************************

    Umbraco database installation script for SQL Server (upgrade from Umbraco 4.0.x)
 
	IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 4.10.0.0
    
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
-- rename cmsTab
RENAME TABLE cmsTab TO cmsPropertyTypeGroup;
;
-- add parent Group to new cmsPropertyTypeGroup
ALTER TABLE cmsPropertyTypeGroup
ADD parentGroupId int NULL DEFAULT NULL
;
ALTER TABLE cmsPropertyType 
ADD FOREIGN KEY (propertyTypeGroupId) 
REFERENCES cmsPropertyTypeGroup (id) 
;                                 
-- add sortOrder to cmsContentTypeAllowedContentType
ALTER TABLE cmsContentTypeAllowedContentType
ADD sortOrder int NOT NULL DEFAULT 1
;
-- add container and allowAtRoot to cmsContentType
ALTER TABLE cmsContentType
ADD isContainer int NOT NULL DEFAULT 0
;
ALTER TABLE cmsContentType
ADD allowAtRoot int NOT NULL DEFAULT 0
;
CREATE TABLE cmsContentType2ContentType 
(parentContentTypeId int NOT NULL, childContentTypeId int NOT NULL) 
; 
ALTER TABLE cmsContentType2ContentType ADD CONSTRAINT PK_cmsContentType2ContentType PRIMARY KEY CLUSTERED  (parentContentTypeId, childContentTypeId) 
;
-- move all masterContentType information to new cmsContentType2ContentType table
INSERT INTO cmsContentType2ContentType (parentContentTypeId, childContentTypeId)
select masterContentType, nodeId from cmsContentType WHERE not masterContentType is null and masterContentType != 0
;
-- remove masterContentType column from cmsContentType now that it's replaced by a separate table
ALTER TABLE cmsContentType DROP COLUMN masterContentType
;
-- rename tab to propertyGroup on propertyType
ALTER TABLE cmsPropertyType ADD propertyTypeGroupId int
;
UPDATE cmsPropertyType SET propertyTypeGroupId = tabId
;
ALTER TABLE cmsPropertyType DROP FOREIGN KEY FK_cmsPropertyType_cmsTab
;
ALTER TABLE cmsPropertyType DROP COLUMN tabId
;
