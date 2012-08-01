/*

THIS IS A TEMPORARY FILE USED TO STORE THE SCRIPTS TO CREATE FOREIGN KEYS, UNIQUE CONSTRAINTS AND INDEXES FOR PERFORMANCE REASONS.
THIS WILL BE MERGED INTO THE INSTALL SCRIPTS AND WILL BE CREATED AS A SEPERATE STANDALONE UPGRADE SCRIPT FOR OLDER VERSIONS.
THIS FILE EXISTS HERE CURRENTLY BECAUSE A FULL DATA LAYER TEST SUITE NEEDS TO BE CREATED TO ENSURE THAT THESE CONSTRAINTS ARE 
NOT GOING TO BREAK UMBRACO.

*/

/************************** CLEANUP ***********************************************/

/* DELETE NON-EXISTING DOCUMENTS */
delete from cmsDocument where nodeId not in (select id from umbracoNode)
;

/* CLEAN UNUSED CONTENT ROWS */
delete from cmsContent where nodeId not in (select id from umbracoNode)
;

/* CLEAN UNUSED VERSIONS */
delete from cmsContentVersion where contentid not in (select nodeId from cmsContent)
;

/* CLEAN UNUSED XML */
delete from cmsContentXml where nodeid not in (select nodeId from cmsContent)
;

/* CLEAN UNUSED DOCUMENT TYPES */
delete from cmsDocumentType where contentTypeNodeId not in (select nodeId from cmsContentType)
;
delete from cmsDocumentType where templateNodeId not in (select nodeid from cmsTemplate)
;

/* UPDATE EMPTY TEMPLATE REFERENCES IN DOCUMENTS */
update cmsDocument set templateId = NULL where templateId not in (select nodeId from cmsTemplate)
;

/* DELETE ALL NOTIFICATIONS THAT NO LONGER HAVE NODES */
delete from umbracoUser2NodeNotify where nodeId not in (select id from umbracoNode)
;

/* DELETE ALL NOTIFICATIONS THAT NO LONGER HAVE USERS */
delete from umbracoUser2NodeNotify where userId not in (select id from umbracoUser)
;

/* DELETE UMBRACO NODE DATA THAT IS FLAGGED AS A DOCUMENT OBJECT TYPE THAT DOESN'T EXIST IN THE CONTENT TABLE ANY LONGER */
delete from umbracoNode where id not in
(select nodeId from cmsContent) and nodeObjectType = 'c66ba18e-eaf3-4cff-8a22-41b16d66a972'
;

/* DELETE PERMISSIONS THAT RELATED TO NON-EXISTING USERS */ 
delete from umbracoUser2NodePermission where userId not in (select id from umbracoUser)
;

/* DELETE PERMISSIONS THAT RELATED TO NON-EXISTING NODES */
delete from umbracoUser2NodePermission where nodeId not in (select id from umbracoNode)
;

/* SET MASTER TEMPLATE TO NULL WHEN THERE ISN'T ONE SPECIFIED */
update cmsTemplate set [master] = NULL where [master] = 0

/* 
We need to remove any data type that doesn't exist in umbracoNode as these shouldn't actually exist 
I think they must be left over from how Umbraco used to show the types of data types registered instead
of using reflection. Here are the data types in the cmsDataType table that are not in umbracoNode:

12	-91	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
22	-44	A3776494-0574-4D93-B7DE-EFDFDEC6F2D1	Ntext
23	-128	A52C7C1C-C330-476E-8605-D63D3B84B6A6	Nvarchar
24	-129	928639ED-9C73-4028-920C-1E55DBB68783	Nvarchar
25	-130	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
26	-131	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
27	-132	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Nvarchar
28	-133	6C738306-4C17-4D88-B9BD-6546F3771597	Ntext
29	-134	928639ED-9C73-4028-920C-1E55DBB68783	Nvarchar
30	-50	AAF99BB2-DBBE-444D-A296-185076BF0484	Date
39	1042	5E9B75AE-FACE-41C8-B47E-5F4B0FD82F83	Ntext
40	1043	5E9B75AE-FACE-41C8-B47E-5F4B0FD82F83	Ntext
41	1044	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Ntext
42	1045	A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6	Ntext
47	1194	D15E1281-E456-4B24-AA86-1DDA3E4299D5	Ntext

*/
DELETE FROM cmsDataType WHERE nodeId NOT IN (SELECT id FROM umbracoNode)
;

/* Need to remove any data type prevalues that aren't related to a data type */
DELETE FROM cmsDataTypePreValues WHERE dataTypeNodeID NOT IN (SELECT nodeId FROM cmsDataType)
;

/* Remove any domains that should not exist as they weren't deleted before when documents were deleted */
DELETE FROM umbracoDomains WHERE domainRootStructureId NOT IN (SELECT id FROM umbracoNode)
;

-- It would be good to add constraints from cmsLanguageText to umbracoLanguage but unfortunately, a 'zero' id 
-- is entered into cmsLanguageText when a new entry is made, since there's not language with id of zero this won't work.
-- However, we need to remove translations that aren't related to a language (these would be left over from deleting a language)
DELETE FROM cmsLanguageText
WHERE languageId <> 0 AND languageId NOT IN (SELECT id FROM umbracoLanguage)
;

/* need to remove any content restrictions that don't exist in cmsContent */

DELETE FROM cmsContentTypeAllowedContentType WHERE id NOT IN (SELECT nodeId FROM cmsContentType)
;
DELETE FROM cmsContentTypeAllowedContentType WHERE Allowedid NOT IN (SELECT nodeId FROM cmsContentType)
;

/* Though this should not have to be run because it's a new install, you need to clean the previews if you've been testing before the RC */
DELETE FROM cmsPreviewXml WHERE VersionID NOT IN (SELECT VersionId FROM cmsContentVersion)
;

/* Though this should not have to run because it's a new install, you need to remove this constraint if you've been testing with the RC */
IF EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_cmsMemberType')
ALTER TABLE [cmsMemberType] DROP CONSTRAINT [IX_cmsMemberType]
;

/************************** CLEANUP END ********************************************/


/* Create missing indexes and primary keys */
CREATE NONCLUSTERED INDEX [IX_Icon] ON CMSContenttype(nodeId, Icon)
;

ALTER TABLE cmsContentType ADD CONSTRAINT
	IX_cmsContentType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsContent ADD CONSTRAINT
	IX_cmsContent UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsContentVersion ADD CONSTRAINT
	FK_cmsContentVersion_cmsContent FOREIGN KEY
	(
	ContentId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember ADD CONSTRAINT
	PK_cmsMember PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsMember ADD CONSTRAINT
	FK_cmsMember_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember ADD CONSTRAINT
	FK_cmsMember_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsStylesheet ADD CONSTRAINT
	PK_cmsStylesheet PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsStylesheetProperty ADD CONSTRAINT
	PK_cmsStylesheetProperty PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsStylesheetProperty ADD CONSTRAINT
	FK_cmsStylesheetProperty_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsStylesheet ADD CONSTRAINT
	FK_cmsStylesheet_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentXml ADD CONSTRAINT
	FK_cmsContentXml_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDataType ADD CONSTRAINT
	IX_cmsDataType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;



ALTER TABLE cmsDataType ADD CONSTRAINT
	FK_cmsDataType_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE cmsDataTypePreValues ADD CONSTRAINT
	FK_cmsDataTypePreValues_cmsDataType FOREIGN KEY
	(
	datatypeNodeId
	) REFERENCES cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_cmsContentType FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_umbracoNode FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMacroProperty ADD CONSTRAINT
	FK_cmsMacroProperty_cmsMacro FOREIGN KEY
	(
	macro
	) REFERENCES cmsMacro
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_cmsContentType FOREIGN KEY
	(
	NodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_umbracoNode FOREIGN KEY
	(
	NodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember2MemberGroup ADD CONSTRAINT
	FK_cmsMember2MemberGroup_cmsMember FOREIGN KEY
	(
	Member
	) REFERENCES cmsMember
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	IX_cmsDocument UNIQUE NONCLUSTERED 
	(
	nodeId,
	versionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsPropertyData ADD CONSTRAINT
	FK_cmsPropertyData_cmsPropertyType FOREIGN KEY
	(
	propertytypeid
	) REFERENCES cmsPropertyType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsContentType FOREIGN KEY
	(
	contentTypeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsDataType FOREIGN KEY
	(
	dataTypeId
	) REFERENCES cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTab ADD CONSTRAINT
	FK_cmsTab_cmsContentType FOREIGN KEY
	(
	contenttypeNodeId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTemplate ADD CONSTRAINT
	IX_cmsTemplate UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsTemplate FOREIGN KEY
	(
	templateId
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;


ALTER TABLE umbracoDomains ADD CONSTRAINT
	FK_umbracoDomains_umbracoNode FOREIGN KEY
	(
	domainRootStructureID
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsDictionary ADD CONSTRAINT
	IX_cmsDictionary UNIQUE NONCLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsLanguageText ADD CONSTRAINT
	FK_cmsLanguageText_cmsDictionary FOREIGN KEY
	(
	UniqueId
	) REFERENCES cmsDictionary
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser FOREIGN KEY
	(
	parentUserId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser1 FOREIGN KEY
	(
	userId
	) REFERENCES umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

CREATE NONCLUSTERED INDEX IX_umbracoLog ON umbracoLog
	(
	NodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoRelationType FOREIGN KEY
	(
	relType
	) REFERENCES umbracoRelationType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode FOREIGN KEY
	(
	parentId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode1 FOREIGN KEY
	(
	childId
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;



ALTER TABLE cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType FOREIGN KEY
	(
	Id
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType1 FOREIGN KEY
	(
	AllowedId
	) REFERENCES cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE umbracoLanguage ADD CONSTRAINT
	IX_umbracoLanguage UNIQUE NONCLUSTERED 
	(
	languageISOCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE umbracoUser ADD CONSTRAINT
	IX_umbracoUser UNIQUE NONCLUSTERED 
	(
	userLogin
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsTaskType ADD CONSTRAINT
	IX_cmsTaskType UNIQUE NONCLUSTERED 
	(
	alias
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_cmsTemplate FOREIGN KEY
	(
	templateNodeId
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsTemplate ADD CONSTRAINT
	FK_cmsTemplate_cmsTemplate FOREIGN KEY
	(
	master
	) REFERENCES cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsContentVersion ADD CONSTRAINT
	IX_cmsContentVersion UNIQUE NONCLUSTERED 
	(
	VersionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
;

ALTER TABLE cmsPreviewXml ADD CONSTRAINT
	FK_cmsPreviewXml_cmsContentVersion FOREIGN KEY
	(
	versionId
	) REFERENCES cmsContentVersion
	(
	VersionId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsPreviewXml ADD CONSTRAINT
	FK_cmsPreviewXml_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE cmsMember2MemberGroup ADD CONSTRAINT
	FK_cmsMember2MemberGroup_umbracoNode FOREIGN KEY
	(
	MemberGroup
	) REFERENCES umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;