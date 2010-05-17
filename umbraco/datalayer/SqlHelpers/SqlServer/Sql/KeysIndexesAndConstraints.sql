/*

THIS IS A TEMPORARY FILE USED TO STORE THE SCRIPTS TO CREATE FOREIGN KEYS, UNIQUE CONSTRAINTS AND INDEXES FOR PERFORMANCE REASONS.
THIS WILL BE MERGED INTO THE INSTALL SCRIPTS AND WILL BE CREATED AS A SEPERATE STANDALONE UPGRADE SCRIPT FOR OLDER VERSIONS.
THIS FILE EXISTS HERE CURRENTLY BECAUSE A FULL DATA LAYER TEST SUITE NEEDS TO BE CREATED TO ENSURE THAT THESE CONSTRAINTS ARE 
NOT GOING TO BREAK UMBRACO.

*/

/* Create missing indexes and primary keys */
CREATE NONCLUSTERED INDEX [IX_Icon] ON CMSContenttype(nodeId, Icon)
;

ALTER TABLE dbo.cmsContentType ADD CONSTRAINT
	IX_cmsContentType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsContent ADD CONSTRAINT
	IX_cmsContent UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


ALTER TABLE dbo.cmsContentVersion ADD CONSTRAINT
	FK_cmsContentVersion_cmsContent FOREIGN KEY
	(
	ContentId
	) REFERENCES dbo.cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMember ADD CONSTRAINT
	PK_cmsMember PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsMember ADD CONSTRAINT
	FK_cmsMember_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMember ADD CONSTRAINT
	FK_cmsMember_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsStylesheet ADD CONSTRAINT
	PK_cmsStylesheet PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsStylesheetProperty ADD CONSTRAINT
	PK_cmsStylesheetProperty PRIMARY KEY CLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsStylesheetProperty ADD CONSTRAINT
	FK_cmsStylesheetProperty_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsStylesheet ADD CONSTRAINT
	FK_cmsStylesheet_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsContentXml ADD CONSTRAINT
	FK_cmsContentXml_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDataType ADD CONSTRAINT
	IX_cmsDataType UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

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

ALTER TABLE dbo.cmsDataType ADD CONSTRAINT
	FK_cmsDataType_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

/* Need to remove any data type prevalues that aren't related to a data type */
DELETE FROM cmsDataTypePreValues WHERE dataTypeNodeID NOT IN (SELECT nodeId FROM cmsDataType)
;

ALTER TABLE dbo.cmsDataTypePreValues ADD CONSTRAINT
	FK_cmsDataTypePreValues_cmsDataType FOREIGN KEY
	(
	datatypeNodeId
	) REFERENCES dbo.cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsContent FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.cmsContent
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_cmsContentType FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDocumentType ADD CONSTRAINT
	FK_cmsDocumentType_umbracoNode FOREIGN KEY
	(
	contentTypeNodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMacroProperty ADD CONSTRAINT
	FK_cmsMacroProperty_cmsMacro FOREIGN KEY
	(
	macro
	) REFERENCES dbo.cmsMacro
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_cmsContentType FOREIGN KEY
	(
	NodeId
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMemberType ADD CONSTRAINT
	FK_cmsMemberType_umbracoNode FOREIGN KEY
	(
	NodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsMember2MemberGroup ADD CONSTRAINT
	FK_cmsMember2MemberGroup_cmsMember FOREIGN KEY
	(
	Member
	) REFERENCES dbo.cmsMember
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDocument ADD CONSTRAINT
	IX_cmsDocument UNIQUE NONCLUSTERED 
	(
	nodeId,
	versionId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsPropertyData ADD CONSTRAINT
	FK_cmsPropertyData_cmsPropertyType FOREIGN KEY
	(
	propertytypeid
	) REFERENCES dbo.cmsPropertyType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsContentType FOREIGN KEY
	(
	contentTypeId
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsPropertyType ADD CONSTRAINT
	FK_cmsPropertyType_cmsDataType FOREIGN KEY
	(
	dataTypeId
	) REFERENCES dbo.cmsDataType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsTab ADD CONSTRAINT
	FK_cmsTab_cmsContentType FOREIGN KEY
	(
	contenttypeNodeId
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsTemplate ADD CONSTRAINT
	IX_cmsTemplate UNIQUE NONCLUSTERED 
	(
	nodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsDocument ADD CONSTRAINT
	FK_cmsDocument_cmsTemplate FOREIGN KEY
	(
	templateId
	) REFERENCES dbo.cmsTemplate
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoDomains ADD CONSTRAINT
	FK_umbracoDomains_umbracoNode FOREIGN KEY
	(
	domainRootStructureID
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDictionary ADD CONSTRAINT
	IX_cmsDictionary UNIQUE NONCLUSTERED 
	(
	id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.cmsLanguageText ADD CONSTRAINT
	FK_cmsLanguageText_cmsDictionary FOREIGN KEY
	(
	UniqueId
	) REFERENCES dbo.cmsDictionary
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES dbo.umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoUser2NodeNotify ADD CONSTRAINT
	FK_umbracoUser2NodeNotify_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoUser FOREIGN KEY
	(
	userId
	) REFERENCES dbo.umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoUser2NodePermission ADD CONSTRAINT
	FK_umbracoUser2NodePermission_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser FOREIGN KEY
	(
	parentUserId
	) REFERENCES dbo.umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoUser1 FOREIGN KEY
	(
	userId
	) REFERENCES dbo.umbracoUser
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsTask ADD CONSTRAINT
	FK_cmsTask_umbracoNode FOREIGN KEY
	(
	nodeId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsDictionary ADD CONSTRAINT
	FK_cmsDictionary_cmsDictionary FOREIGN KEY
	(
	parent
	) REFERENCES dbo.cmsDictionary
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

CREATE NONCLUSTERED INDEX IX_umbracoLog ON dbo.umbracoLog
	(
	NodeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
;

ALTER TABLE dbo.umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoRelationType FOREIGN KEY
	(
	relType
	) REFERENCES dbo.umbracoRelationType
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode FOREIGN KEY
	(
	parentId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.umbracoRelation ADD CONSTRAINT
	FK_umbracoRelation_umbracoNode1 FOREIGN KEY
	(
	childId
	) REFERENCES dbo.umbracoNode
	(
	id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

/* need to remove any content restrictions that don't exist in cmsContent */

DELETE FROM cmsContentTypeAllowedContentType WHERE id NOT IN (SELECT nodeId FROM cmsContentType)
DELETE FROM cmsContentTypeAllowedContentType WHERE Allowedid NOT IN (SELECT nodeId FROM cmsContentType)

ALTER TABLE dbo.cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType FOREIGN KEY
	(
	Id
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;

ALTER TABLE dbo.cmsContentTypeAllowedContentType ADD CONSTRAINT
	FK_cmsContentTypeAllowedContentType_cmsContentType1 FOREIGN KEY
	(
	AllowedId
	) REFERENCES dbo.cmsContentType
	(
	nodeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
;