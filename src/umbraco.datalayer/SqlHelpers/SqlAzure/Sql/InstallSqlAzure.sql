DECLARE @dbEdition sql_variant;

SET @dbEdition = (SELECT SERVERPROPERTY ('edition'));

-- only run the install script for SqlAzure, as when used for WAWS installs
IF( @dbEdition = 'SQL Azure' )
BEGIN
		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [dbo].[UCUserSettings](
			[User] [nvarchar](50) NOT NULL,
			[Key] [nvarchar](250) NOT NULL,
			[Value] [nvarchar](2500) NULL,
		 CONSTRAINT [PK_UCUserSettings] PRIMARY KEY CLUSTERED 
		(
			[User] ASC,
			[Key] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF )
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoUserType](
			[id] [smallint] IDENTITY(1,1) NOT NULL,
			[userTypeAlias] [nvarchar](50) NULL,
			[userTypeName] [nvarchar](255) NOT NULL,
			[userTypeDefaultPermissions] [nvarchar](50) NULL,
		 CONSTRAINT [PK_userType] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [umbracoUserType] ON
		INSERT [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (1, N'admin', N'Administrators', N'CADMOSKTPIURZ:5F')
		INSERT [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (2, N'writer', N'Writer', N'CAH:F')
		INSERT [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (3, N'editor', N'Editors', N'CADMOSKTPUZ:5F')
		INSERT [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (4, N'translator', N'Translator', N'AF')
		SET IDENTITY_INSERT [umbracoUserType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoUserLogins](
			[contextID] [uniqueidentifier] NOT NULL,
			[userID] [int] NOT NULL,
			[timeout] [bigint] NOT NULL
		)
		;
		-- added for SqlAzure
		CREATE CLUSTERED INDEX umbracoUserLogins_Index ON umbracoUserLogins (contextID)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsTaskType](
			[id] [tinyint] IDENTITY(1,1) NOT NULL,
			[alias] [nvarchar](255) NOT NULL,
		 CONSTRAINT [PK_cmsTaskType] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsTaskType] UNIQUE NONCLUSTERED 
		(
			[alias] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsTaskType] ON
		INSERT [cmsTaskType] ([id], [alias]) VALUES (1, N'toTranslate')
		SET IDENTITY_INSERT [cmsTaskType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoRelationType](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[dual] [bit] NOT NULL,
			[parentObjectType] [uniqueidentifier] NOT NULL,
			[childObjectType] [uniqueidentifier] NOT NULL,
			[name] [nvarchar](255) NOT NULL,
			[alias] [nvarchar](100) NULL,
		 CONSTRAINT [PK_umbracoRelationType] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [umbracoRelationType] ON
		INSERT [umbracoRelationType] ([id], [dual], [parentObjectType], [childObjectType], [name], [alias]) VALUES (1, 1, N'c66ba18e-eaf3-4cff-8a22-41b16d66a972', N'c66ba18e-eaf3-4cff-8a22-41b16d66a972', N'Relate Document On Copy', N'relateDocumentOnCopy')
		SET IDENTITY_INSERT [umbracoRelationType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoApp](
			[sortOrder] [tinyint] NOT NULL,
			[appAlias] [nvarchar](50) NOT NULL,
			[appIcon] [nvarchar](255) NOT NULL,
			[appName] [nvarchar](255) NOT NULL,
			[appInitWithTreeAlias] [nvarchar](255) NULL,
		 CONSTRAINT [PK_umbracoApp] PRIMARY KEY CLUSTERED 
		(
			[appAlias] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (0, N'content', N'.traycontent', N'Indhold', N'content')
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (7, N'developer', N'.traydeveloper', N'Developer', NULL)
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (1, N'media', N'.traymedia', N'Mediearkiv', NULL)
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (8, N'member', N'.traymember', N'Medlemmer', NULL)
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (6, N'settings', N'.traysettings', N'Indstillinger', NULL)
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (5, N'translation', N'.traytranslation', N'Translation', NULL)
		INSERT [umbracoApp] ([sortOrder], [appAlias], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (5, N'users', N'.trayusers', N'Brugere', NULL)

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [Comment](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[mainid] [int] NOT NULL,
			[nodeid] [int] NOT NULL,
			[name] [nvarchar](250) NULL,
			[email] [nvarchar](250) NULL,
			[website] [nvarchar](250) NULL,
			[comment] [ntext] NULL,
			[spam] [bit] NULL,
			[ham] [bit] NULL,
			[created] [datetime] NULL,
		 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		) 
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoNode](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[trashed] [bit] NOT NULL,
			[parentID] [int] NOT NULL,
			[nodeUser] [int] NULL,
			[level] [smallint] NOT NULL,
			[path] [nvarchar](150) NOT NULL,
			[sortOrder] [int] NOT NULL,
			[uniqueID] [uniqueidentifier] NULL,
			[text] [nvarchar](255) NULL,
			[nodeObjectType] [uniqueidentifier] NULL,
			[createDate] [datetime] NOT NULL,
		 CONSTRAINT [PK_structure] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [umbracoNode] ON
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-92, 0, -1, 0, 11, N'-1,-92', 37, N'f0bc4bfb-b499-40d6-ba86-058885a5178c', N'Label', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-90, 0, -1, 0, 11, N'-1,-90', 35, N'84c6b441-31df-4ffe-b67e-67d5bc3ae65a', N'Upload', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-89, 0, -1, 0, 11, N'-1,-89', 34, N'c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3', N'Textbox multiple', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-88, 0, -1, 0, 11, N'-1,-88', 33, N'0cc0eba1-9960-42c9-bf9b-60e150b429ae', N'Textstring', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-87, 0, -1, 0, 11, N'-1,-87', 32, N'ca90c950-0aff-4e72-b976-a30b1ac57dad', N'Richtext editor', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-51, 0, -1, 0, 11, N'-1,-51', 4, N'2e6d3631-066e-44b8-aec4-96f09099b2b5', N'Numeric', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-49, 0, -1, 0, 11, N'-1,-49', 2, N'92897bc6-a5f3-4ffe-ae27-f2e7e33dda49', N'True/false', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-43, 0, -1, 0, 1, N'-1,-43', 2, N'fbaf13a8-4036-41f2-93a3-974f678c312a', N'Checkbox list', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9C10E AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-42, 0, -1, 0, 1, N'-1,-42', 2, N'0b6a45e7-44ba-430d-9da5-4e46060b9e03', N'Dropdown', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9BAC4 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-41, 0, -1, 0, 1, N'-1,-41', 2, N'5046194e-4237-453c-a547-15db3a07c4e1', N'Date Picker', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9B543 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-40, 0, -1, 0, 1, N'-1,-40', 2, N'bb5f57c9-ce2b-4bb9-b697-4caca783a805', N'Radiobox', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9AF58 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-39, 0, -1, 0, 1, N'-1,-39', 2, N'f38f0ac7-1d27-439c-9f3f-089cd8825a53', N'Dropdown multiple', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9A9C0 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-38, 0, -1, 0, 1, N'-1,-38', 2, N'fd9f1447-6c61-4a7c-9595-5aa39147d318', N'Folder Browser', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E9A102 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-37, 0, -1, 0, 1, N'-1,-37', 2, N'0225af17-b302-49cb-9176-b9f35cab9c17', N'Approved Color', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E99976 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-36, 0, -1, 0, 1, N'-1,-36', 2, N'e4d66c0f-b935-4200-81f0-025f7256b89a', N'Date Picker with time', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000958100E99096 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-21, 0, -1, 0, 0, N'-1,-21', 0, N'bf7c7cbc-952f-4518-97a2-69e9c7b33842', N'Recycle Bin', N'cf3d8e34-1c1c-41e9-ae56-878b57b32113', CAST(0x0000A05100C7337F AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-20, 0, -1, 0, 0, N'-1,-20', 0, N'0f582a79-1e41-4cf0-bfa0-76340651891a', N'Recycle Bin', N'01bb7ff2-24dc-4c0c-95a2-c24ef72bbac8', CAST(0x0000A05100C73355 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-1, 0, -1, 0, 0, N'-1', 0, N'916724a5-173d-4619-b97e-b9de133dd6f5', N'SYSTEM DATA: umbraco master root', N'ea7d8624-4cfe-4578-a871-24aa946bf34d', CAST(0x0000957200E73750 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1031, 0, -1, 1, 1, N'-1,1031', 2, N'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d', N'Folder', N'4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', CAST(0x000095B00003C1CF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1032, 0, -1, 1, 1, N'-1,1032', 2, N'cc07b313-0843-4aa8-bbda-871c8da728c8', N'Image', N'4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', CAST(0x000095B00003C551 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1033, 0, -1, 1, 1, N'-1,1033', 2, N'4c52d8ab-54e6-40cd-999c-7a5f24903e4d', N'File', N'4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', CAST(0x000095B00003C837 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1034, 0, -1, 0, 1, N'-1,1034', 2, N'a6857c73-d6e9-480c-b6e6-f15f6ad11125', N'Content Picker', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D84A29 AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1035, 0, -1, 0, 1, N'-1,1035', 2, N'93929b9a-93a2-4e2a-b239-d99334440a59', N'Media Picker', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D8524B AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1036, 0, -1, 0, 1, N'-1,1036', 2, N'2b24165f-9782-4aa3-b459-1de4a4d21f60', N'Member Picker', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D8571E AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1038, 0, -1, 0, 1, N'-1,1038', 2, N'1251c96c-185c-4e9b-93f4-b48205573cbd', N'Simple Editor', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1039, 0, -1, 0, 1, N'-1,1039', 2, N'06f349a9-c949-4b6a-8660-59c10451af42', N'Ultimate Picker', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1040, 0, -1, 0, 1, N'-1,1040', 2, N'21e798da-e06e-4eda-a511-ed257f78d4fa', N'Related Links', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1041, 0, -1, 0, 1, N'-1,1041', 2, N'b6b73142-b9c1-4bf8-a16d-e1c23320b549', N'Tags', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1042, 0, -1, 0, 1, N'-1,1042', 2, N'0a452bd5-83f9-4bc3-8403-1286e13fb77e', N'Macro Container', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		INSERT [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1043, 0, -1, 0, 1, N'-1,1043', 2, N'1df9f033-e6d4-451f-b8d2-e0cbc50a836f', N'Image Cropper', N'30a2a501-1978-4ddb-a57b-f7efed43ba3c', CAST(0x0000973E00D868AF AS DateTime))
		SET IDENTITY_INSERT [umbracoNode] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoLog](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[userId] [int] NOT NULL,
			[NodeId] [int] NOT NULL,
			[Datestamp] [datetime] NOT NULL,
			[logHeader] [nvarchar](50) NOT NULL,
			[logComment] [nvarchar](4000) NULL,
		 CONSTRAINT [PK_umbracoLog] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoLanguage](
			[id] [smallint] IDENTITY(1,1) NOT NULL,
			[languageISOCode] [nvarchar](10) NULL,
			[languageCultureName] [nvarchar](100) NULL,
		 CONSTRAINT [PK_language] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_umbracoLanguage] UNIQUE NONCLUSTERED 
		(
			[languageISOCode] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [umbracoLanguage] ON
		INSERT [umbracoLanguage] ([id], [languageISOCode], [languageCultureName]) VALUES (1, N'en-US', N'en-US')
		SET IDENTITY_INSERT [umbracoLanguage] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMacro](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[macroUseInEditor] [bit] NOT NULL,
			[macroRefreshRate] [int] NOT NULL,
			[macroAlias] [nvarchar](255) NOT NULL,
			[macroName] [nvarchar](255) NULL,
			[macroScriptType] [nvarchar](255) NULL,
			[macroScriptAssembly] [nvarchar](255) NULL,
			[macroXSLT] [nvarchar](255) NULL,
			[macroCacheByPage] [bit] NOT NULL,
			[macroCachePersonalized] [bit] NOT NULL,
			[macroDontRender] [bit] NOT NULL,
			[macroPython] [nvarchar](255) NULL,
		 CONSTRAINT [PK_macro] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		SET ANSI_PADDING ON
		;
		CREATE TABLE [cmsTags](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[tag] [varchar](200) NULL,
			[parentId] [int] NULL,
			[group] [varchar](100) NULL,
		 CONSTRAINT [PK_cmsTags] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET ANSI_PADDING OFF
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsDictionary](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[id] [uniqueidentifier] NOT NULL,
			[parent] [uniqueidentifier] NOT NULL,
			[key] [nvarchar](1000) NOT NULL,
		 CONSTRAINT [PK_cmsDictionary] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsDictionary] UNIQUE NONCLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMacroPropertyType](
			[id] [smallint] IDENTITY(1,1) NOT NULL,
			[macroPropertyTypeAlias] [nvarchar](50) NULL,
			[macroPropertyTypeRenderAssembly] [nvarchar](255) NULL,
			[macroPropertyTypeRenderType] [nvarchar](255) NULL,
			[macroPropertyTypeBaseType] [nvarchar](255) NULL,
		 CONSTRAINT [PK_macroPropertyType] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsMacroPropertyType] ON
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (3, N'mediaCurrent', N'umbraco.macroRenderings', N'media', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (4, N'contentSubs', N'umbraco.macroRenderings', N'content', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (5, N'contentRandom', N'umbraco.macroRenderings', N'content', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (6, N'contentPicker', N'umbraco.macroRenderings', N'content', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (13, N'number', N'umbraco.macroRenderings', N'numeric', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (14, N'bool', N'umbraco.macroRenderings', N'yesNo', N'Boolean')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (16, N'text', N'umbraco.macroRenderings', N'text', N'String')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (17, N'contentTree', N'umbraco.macroRenderings', N'content', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (18, N'contentType', N'umbraco.macroRenderings', N'contentTypeSingle', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (19, N'contentTypeMultiple', N'umbraco.macroRenderings', N'contentTypeMultiple', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (20, N'contentAll', N'umbraco.macroRenderings', N'content', N'Int32')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (21, N'tabPicker', N'umbraco.macroRenderings', N'tabPicker', N'String')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (22, N'tabPickerMultiple', N'umbraco.macroRenderings', N'tabPickerMultiple', N'String')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (23, N'propertyTypePicker', N'umbraco.macroRenderings', N'propertyTypePicker', N'String')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (24, N'propertyTypePickerMultiple', N'umbraco.macroRenderings', N'propertyTypePickerMultiple', N'String')
		INSERT [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (25, N'textMultiLine', N'umbraco.macroRenderings', N'textMultiple', N'String')
		SET IDENTITY_INSERT [cmsMacroPropertyType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMacroProperty](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[macroPropertyHidden] [bit] NOT NULL,
			[macroPropertyType] [smallint] NOT NULL,
			[macro] [int] NOT NULL,
			[macroPropertySortOrder] [tinyint] NOT NULL,
			[macroPropertyAlias] [nvarchar](50) NOT NULL,
			[macroPropertyName] [nvarchar](255) NOT NULL,
		 CONSTRAINT [PK_macroProperty] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsStylesheetProperty](
			[nodeId] [int] NOT NULL,
			[stylesheetPropertyEditor] [bit] NULL,
			[stylesheetPropertyAlias] [nvarchar](50) NULL,
			[stylesheetPropertyValue] [nvarchar](400) NULL,
		 CONSTRAINT [PK_cmsStylesheetProperty] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsStylesheet](
			[nodeId] [int] NOT NULL,
			[filename] [nvarchar](100) NOT NULL,
			[content] [ntext] NULL,
		 CONSTRAINT [PK_cmsStylesheet] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		) 
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsTagRelationship](
			[nodeId] [int] NOT NULL,
			[tagId] [int] NOT NULL,
		 CONSTRAINT [PK_cmsTagRelationship] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC,
			[tagId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsContentType](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[nodeId] [int] NOT NULL,
			[alias] [nvarchar](255) NULL,
			[icon] [nvarchar](255) NULL,
			[thumbnail] [nvarchar](255) NOT NULL,
			[description] [nvarchar](1500) NULL,
			[masterContentType] [int] NULL,
		 CONSTRAINT [PK_cmsContentType] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsContentType] UNIQUE NONCLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsContentType] ON
		INSERT [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail], [description], [masterContentType]) VALUES (532, 1031, N'Folder', N'folder.gif', N'folder.png', NULL, NULL)
		INSERT [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail], [description], [masterContentType]) VALUES (533, 1032, N'Image', N'mediaPhoto.gif', N'folder.png', NULL, NULL)
		INSERT [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail], [description], [masterContentType]) VALUES (534, 1033, N'File', N'mediaMulti.gif', N'folder.png', NULL, NULL)
		SET IDENTITY_INSERT [cmsContentType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsContent](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[nodeId] [int] NOT NULL,
			[contentType] [int] NOT NULL,
		 CONSTRAINT [PK_cmsContent] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsContent] UNIQUE NONCLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		SET ANSI_PADDING ON
		;
		CREATE TABLE [cmsDataType](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[nodeId] [int] NOT NULL,
			[controlId] [uniqueidentifier] NOT NULL,
			[dbType] [varchar](50) NOT NULL,
		 CONSTRAINT [PK_cmsDataType] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsDataType] UNIQUE NONCLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET ANSI_PADDING OFF
		;
		SET IDENTITY_INSERT [cmsDataType] ON
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (4, -49, N'38b352c1-e9f8-4fd8-9324-9a2eab06d97a', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (6, -51, N'1413afcb-d19a-4173-8e9a-68288d2a73b8', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (8, -87, N'5e9b75ae-face-41c8-b47e-5f4b0fd82f83', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (9, -88, N'ec15c1e5-9d90-422a-aa52-4f7622c63bea', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (10, -89, N'67db8357-ef57-493e-91ac-936d305e0f2a', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (11, -90, N'5032a6e6-69e3-491d-bb28-cd31cd11086c', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (13, -92, N'6c738306-4c17-4d88-b9bd-6546f3771597', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (14, -36, N'b6fb1622-afa5-4bbf-a3cc-d9672a442222', N'Date')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (15, -37, N'f8d60f68-ec59-4974-b43b-c46eb5677985', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (16, -38, N'cccd4ae9-f399-4ed2-8038-2e88d19e810c', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (17, -39, N'928639ed-9c73-4028-920c-1e55dbb68783', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (18, -40, N'a52c7c1c-c330-476e-8605-d63d3b84b6a6', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (19, -41, N'23e93522-3200-44e2-9f29-e61a6fcbb79a', N'Date')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (20, -42, N'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (21, -43, N'b4471851-82b6-4c75-afa4-39fa9c6a75e9', N'Nvarchar')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (31, 1034, N'158aa029-24ed-4948-939e-c3da209e5fba', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (32, 1035, N'ead69342-f06d-4253-83ac-28000225583b', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (33, 1036, N'39f533e4-0551-4505-a64b-e0425c5ce775', N'Integer')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (35, 1038, N'60b7dabf-99cd-41eb-b8e9-4d2e669bbde9', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (36, 1039, N'cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (37, 1040, N'71b8ad1a-8dc2-425c-b6b8-faa158075e63', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (38, 1041, N'4023e540-92f5-11dd-ad8b-0800200c9a66', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (39, 1042, N'474fcff8-9d2d-11de-abc6-ad7a56d89593', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (40, 1043, N'7a2d436c-34c2-410f-898f-4a23b3d79f54', N'Ntext')
		INSERT [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (41, 1044, N'e66af4a0-e8b4-11de-8a39-0800200c9a66', N'Ntext')
		SET IDENTITY_INSERT [cmsDataType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsLanguageText](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[languageId] [int] NOT NULL,
			[UniqueId] [uniqueidentifier] NOT NULL,
			[value] [nvarchar](1000) NOT NULL,
		 CONSTRAINT [PK_cmsLanguageText] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoDomains](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[domainDefaultLanguage] [int] NULL,
			[domainRootStructureID] [int] NULL,
			[domainName] [nvarchar](255) NOT NULL,
		 CONSTRAINT [PK_domains] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoAppTree](
			[treeSilent] [bit] NOT NULL,
			[treeInitialize] [bit] NOT NULL,
			[treeSortOrder] [tinyint] NOT NULL,
			[appAlias] [nvarchar](50) NOT NULL,
			[treeAlias] [nvarchar](150) NOT NULL,
			[treeTitle] [nvarchar](255) NOT NULL,
			[treeIconClosed] [nvarchar](255) NOT NULL,
			[treeIconOpen] [nvarchar](255) NOT NULL,
			[treeHandlerAssembly] [nvarchar](255) NOT NULL,
			[treeHandlerType] [nvarchar](255) NOT NULL,
			[action] [nvarchar](255) NULL,
		 CONSTRAINT [PK_umbracoAppTree] PRIMARY KEY CLUSTERED 
		(
			[appAlias] ASC,
			[treeAlias] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (1, 1, 0, N'content', N'content', N'Indhold', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadContent', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 0, 0, N'content', N'contentRecycleBin', N'RecycleBin', N'folder.gif', N'folder_o.gif', N'umbraco', N'cms.presentation.Trees.ContentRecycleBin', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 0, N'developer', N'cacheBrowser', N'CacheBrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCache', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 0, 0, N'developer', N'CacheItem', N'Cachebrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCacheItem', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 1, N'developer', N'datatype', N'Datatyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDataTypes', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 2, N'developer', N'macros', N'Macros', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMacros', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 3, N'developer', N'packager', N'Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackager', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 0, 1, N'developer', N'packagerPackages', N'Packager Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackages', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 4, N'developer', N'python', N'Python Files', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPython', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 5, N'developer', N'xslt', N'XSLT Files', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadXslt', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 0, N'media', N'media', N'Medier', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMedia', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 0, 0, N'media', N'mediaRecycleBin', N'RecycleBin', N'folder.gif', N'folder_o.gif', N'umbraco', N'cms.presentation.Trees.MediaRecycleBin', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 0, N'member', N'member', N'Medlemmer', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMembers', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 1, N'member', N'memberGroup', N'MemberGroups', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberGroups', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 2, N'member', N'memberType', N'Medlemstyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberTypes', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 3, N'settings', N'dictionary', N'Dictionary', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDictionary', N'openDictionary()')
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 4, N'settings', N'languages', N'Languages', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadLanguages', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 5, N'settings', N'mediaTypes', N'Medietyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMediaTypes', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 6, N'settings', N'nodeTypes', N'Dokumenttyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadNodeTypes', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 2, N'settings', N'scripts', N'Scripts', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadScripts', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 0, 0, N'settings', N'stylesheetProperty', N'Stylesheet Property', N'', N'', N'umbraco', N'loadStylesheetProperty', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 0, N'settings', N'stylesheets', N'Stylesheets', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadStylesheets', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 1, N'settings', N'templates', N'Templates', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadTemplates', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 1, N'translation', N'openTasks', N'Tasks assigned to you', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadOpenTasks', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 2, N'translation', N'yourTasks', N'Tasks created by you', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadYourTasks', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 2, N'users', N'userPermissions', N'User Permissions', N'folder.gif', N'folder_o.gif', N'umbraco', N'cms.presentation.Trees.UserPermissions', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 0, N'users', N'users', N'Brugere', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadUsers', NULL)
		INSERT [umbracoAppTree] ([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (0, 1, 1, N'users', N'userTypes', N'User Types', N'folder.gif', N'folder_o.gif', N'umbraco', N'cms.presentation.Trees.UserTypes', NULL)

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsTemplate](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[nodeId] [int] NOT NULL,
			[master] [int] NULL,
			[alias] [nvarchar](100) NULL,
			[design] [ntext] NOT NULL,
		 CONSTRAINT [PK_templates] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsTemplate] UNIQUE NONCLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		) 
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoRelation](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[parentId] [int] NOT NULL,
			[childId] [int] NOT NULL,
			[relType] [int] NOT NULL,
			[datetime] [datetime] NOT NULL,
			[comment] [nvarchar](1000) NOT NULL,
		 CONSTRAINT [PK_umbracoRelation] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoUser](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[userDisabled] [bit] NOT NULL,
			[userNoConsole] [bit] NOT NULL,
			[userType] [smallint] NOT NULL,
			[startStructureID] [int] NOT NULL,
			[startMediaID] [int] NULL,
			[userName] [nvarchar](255) NOT NULL,
			[userLogin] [nvarchar](125) NOT NULL,
			[userPassword] [nvarchar](125) NOT NULL,
			[userEmail] [nvarchar](255) NOT NULL,
			[userDefaultPermissions] [nvarchar](50) NULL,
			[userLanguage] [nvarchar](10) NULL,
			[defaultToLiveEditing] [bit] NOT NULL,
		 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_umbracoUser] UNIQUE NONCLUSTERED 
		(
			[userLogin] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		SET ANSI_PADDING ON
		;
		CREATE TABLE [umbracoUser2NodePermission](
			[userId] [int] NOT NULL,
			[nodeId] [int] NOT NULL,
			[permission] [char](1) NOT NULL,
		 CONSTRAINT [PK_umbracoUser2NodePermission] PRIMARY KEY CLUSTERED 
		(
			[userId] ASC,
			[nodeId] ASC,
			[permission] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET ANSI_PADDING OFF
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		SET ANSI_PADDING ON
		;
		CREATE TABLE [umbracoUser2NodeNotify](
			[userId] [int] NOT NULL,
			[nodeId] [int] NOT NULL,
			[action] [char](1) NOT NULL,
		 CONSTRAINT [PK_umbracoUser2NodeNotify] PRIMARY KEY CLUSTERED 
		(
			[userId] ASC,
			[nodeId] ASC,
			[action] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET ANSI_PADDING OFF
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [umbracoUser2app](
			[user] [int] NOT NULL,
			[app] [nvarchar](50) NOT NULL,
		 CONSTRAINT [PK_user2app] PRIMARY KEY CLUSTERED 
		(
			[user] ASC,
			[app] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'content')
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'developer')
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'media')
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'member')
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'settings')
		INSERT [umbracoUser2app] ([user], [app]) VALUES (0, N'users')

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsTask](
			[closed] [bit] NOT NULL,
			[id] [int] IDENTITY(1,1) NOT NULL,
			[taskTypeId] [tinyint] NOT NULL,
			[nodeId] [int] NOT NULL,
			[parentUserId] [int] NOT NULL,
			[userId] [int] NOT NULL,
			[DateTime] [datetime] NOT NULL,
			[Comment] [nvarchar](500) NULL,
		 CONSTRAINT [PK_cmsTask] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsDocumentType](
			[contentTypeNodeId] [int] NOT NULL,
			[templateNodeId] [int] NOT NULL,
			[IsDefault] [bit] NOT NULL,
		 CONSTRAINT [PK_cmsDocumentType] PRIMARY KEY CLUSTERED 
		(
			[contentTypeNodeId] ASC,
			[templateNodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		INSERT [cmsDocumentType] ([contentTypeNodeId], [templateNodeId], [IsDefault]) VALUES (1052, 1051, 1)
		INSERT [cmsDocumentType] ([contentTypeNodeId], [templateNodeId], [IsDefault]) VALUES (1053, 1046, 1)
		INSERT [cmsDocumentType] ([contentTypeNodeId], [templateNodeId], [IsDefault]) VALUES (1054, 1045, 1)
		INSERT [cmsDocumentType] ([contentTypeNodeId], [templateNodeId], [IsDefault]) VALUES (1055, 1050, 1)

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsDocument](
			[nodeId] [int] NOT NULL,
			[published] [bit] NOT NULL,
			[documentUser] [int] NOT NULL,
			[versionId] [uniqueidentifier] NOT NULL,
			[text] [nvarchar](255) NOT NULL,
			[releaseDate] [datetime] NULL,
			[expireDate] [datetime] NULL,
			[updateDate] [datetime] NOT NULL,
			[templateId] [int] NULL,
			[alias] [nvarchar](255) NULL,
			[newest] [bit] NOT NULL,
		 CONSTRAINT [PK_cmsDocument] PRIMARY KEY CLUSTERED 
		(
			[versionId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsDocument] UNIQUE NONCLUSTERED 
		(
			[nodeId] ASC,
			[versionId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsContentXml](
			[nodeId] [int] NOT NULL,
			[xml] [ntext] NOT NULL,
		 CONSTRAINT [PK_cmsContentXml] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		) 
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsContentVersion](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[ContentId] [int] NOT NULL,
			[VersionId] [uniqueidentifier] NOT NULL,
			[VersionDate] [datetime] NOT NULL,
		PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF),
		 CONSTRAINT [IX_cmsContentVersion] UNIQUE NONCLUSTERED 
		(
			[VersionId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsContentTypeAllowedContentType](
			[Id] [int] NOT NULL,
			[AllowedId] [int] NOT NULL,
		 CONSTRAINT [PK_cmsContentTypeAllowedContentType] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC,
			[AllowedId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsTab](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[contenttypeNodeId] [int] NOT NULL,
			[text] [nvarchar](255) NOT NULL,
			[sortorder] [int] NOT NULL,
		 CONSTRAINT [PK_cmsTab] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsTab] ON
		INSERT [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (3, 1032, N'Image', 1)
		INSERT [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (4, 1033, N'File', 1)
		INSERT [cmsTab] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (5, 1031, N'Contents', 1)
		SET IDENTITY_INSERT [cmsTab] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsDataTypePreValues](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[datatypeNodeId] [int] NOT NULL,
			[value] [nvarchar](2500) NULL,
			[sortorder] [int] NOT NULL,
			[alias] [nvarchar](50) NULL,
		 CONSTRAINT [PK_cmsDataTypePreValues] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsDataTypePreValues] ON
		INSERT [cmsDataTypePreValues] ([id], [datatypeNodeId], [value], [sortorder], [alias]) VALUES (3, -87, N',code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,|1|1,2,3,|0|500,400|1049,|true|', 0, N'')
		INSERT [cmsDataTypePreValues] ([id], [datatypeNodeId], [value], [sortorder], [alias]) VALUES (4, 1041, N'default', 0, N'group')
		SET IDENTITY_INSERT [cmsDataTypePreValues] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMember](
			[nodeId] [int] NOT NULL,
			[Email] [nvarchar](1000) NOT NULL,
			[LoginName] [nvarchar](1000) NOT NULL,
			[Password] [nvarchar](1000) NOT NULL,
		 CONSTRAINT [PK_cmsMember] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMemberType](
			[pk] [int] IDENTITY(1,1) NOT NULL,
			[NodeId] [int] NOT NULL,
			[propertytypeId] [int] NOT NULL,
			[memberCanEdit] [bit] NOT NULL,
			[viewOnProfile] [bit] NOT NULL,
		 CONSTRAINT [PK_cmsMemberType] PRIMARY KEY CLUSTERED 
		(
			[pk] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsPreviewXml](
			[nodeId] [int] NOT NULL,
			[versionId] [uniqueidentifier] NOT NULL,
			[timestamp] [datetime] NOT NULL,
			[xml] [ntext] NOT NULL,
		 CONSTRAINT [PK_cmsContentPreviewXml] PRIMARY KEY CLUSTERED 
		(
			[nodeId] ASC,
			[versionId] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		) 
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsMember2MemberGroup](
			[Member] [int] NOT NULL,
			[MemberGroup] [int] NOT NULL,
		 CONSTRAINT [PK_cmsMember2MemberGroup] PRIMARY KEY CLUSTERED 
		(
			[Member] ASC,
			[MemberGroup] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsPropertyType](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[dataTypeId] [int] NOT NULL,
			[contentTypeId] [int] NOT NULL,
			[tabId] [int] NULL,
			[Alias] [nvarchar](255) NOT NULL,
			[Name] [nvarchar](255) NULL,
			[helpText] [nvarchar](1000) NULL,
			[sortOrder] [int] NOT NULL,
			[mandatory] [bit] NOT NULL,
			[validationRegExp] [nvarchar](255) NULL,
			[Description] [nvarchar](2000) NULL,
		 CONSTRAINT [PK_cmsPropertyType] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;
		SET IDENTITY_INSERT [cmsPropertyType] ON
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (6, -90, 1032, 3, N'umbracoFile', N'Upload image', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (7, -92, 1032, 3, N'umbracoWidth', N'Width', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (8, -92, 1032, 3, N'umbracoHeight', N'Height', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (9, -92, 1032, 3, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (10, -92, 1032, 3, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (24, -90, 1033, 4, N'umbracoFile', N'Upload file', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (25, -92, 1033, 4, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (26, -92, 1033, 4, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL)
		INSERT [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [tabId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (27, -38, 1031, 5, N'contents', N'Contents:', NULL, 0, 0, NULL, NULL)
		SET IDENTITY_INSERT [cmsPropertyType] OFF

		SET ANSI_NULLS ON
		;
		SET QUOTED_IDENTIFIER ON
		;
		CREATE TABLE [cmsPropertyData](
			[id] [int] IDENTITY(1,1) NOT NULL,
			[contentNodeId] [int] NOT NULL,
			[versionId] [uniqueidentifier] NULL,
			[propertytypeid] [int] NOT NULL,
			[dataInt] [int] NULL,
			[dataDate] [datetime] NULL,
			[dataNvarchar] [nvarchar](500) NULL,
			[dataNtext] [ntext] NULL,
		 CONSTRAINT [PK_cmsPropertyData] PRIMARY KEY CLUSTERED 
		(
			[id] ASC
		)WITH ( STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF)
		)
		;

		ALTER TABLE [cmsContentType] ADD  CONSTRAINT [DF_cmsContentType_thumbnail]  DEFAULT ('folder.png') FOR [thumbnail]
		;

		ALTER TABLE [cmsContentType] ADD  CONSTRAINT [DF_cmsContentType_masterContentType]  DEFAULT ((0)) FOR [masterContentType]
		;

		ALTER TABLE [cmsContentVersion] ADD  CONSTRAINT [DF_cmsContentVersion_VersionDate]  DEFAULT (getdate()) FOR [VersionDate]
		;

		ALTER TABLE [cmsDocument] ADD  CONSTRAINT [DF_cmsDocument_updateDate]  DEFAULT (getdate()) FOR [updateDate]
		;

		ALTER TABLE [cmsDocument] ADD  CONSTRAINT [DF_cmsDocument_newest]  DEFAULT ((0)) FOR [newest]
		;

		ALTER TABLE [cmsDocumentType] ADD  CONSTRAINT [DF_cmsDocumentType_IsDefault]  DEFAULT ((0)) FOR [IsDefault]
		;

		ALTER TABLE [cmsMacro] ADD  CONSTRAINT [DF_macro_macroUseInEditor]  DEFAULT ((0)) FOR [macroUseInEditor]
		;

		ALTER TABLE [cmsMacro] ADD  CONSTRAINT [DF_macro_macroRefreshRate]  DEFAULT ((0)) FOR [macroRefreshRate]
		;

		ALTER TABLE [cmsMacro] ADD  CONSTRAINT [DF_cmsMacro_macroCacheByPage]  DEFAULT ((1)) FOR [macroCacheByPage]
		;

		ALTER TABLE [cmsMacro] ADD  CONSTRAINT [DF_cmsMacro_macroCachePersonalized]  DEFAULT ((0)) FOR [macroCachePersonalized]
		;

		ALTER TABLE [cmsMacro] ADD  CONSTRAINT [DF_cmsMacro_macroDontRender]  DEFAULT ((0)) FOR [macroDontRender]
		;

		ALTER TABLE [cmsMacroProperty] ADD  CONSTRAINT [DF_macroProperty_macroPropertyHidden]  DEFAULT ((0)) FOR [macroPropertyHidden]
		;

		ALTER TABLE [cmsMacroProperty] ADD  CONSTRAINT [DF_macroProperty_macroPropertySortOrder]  DEFAULT ((0)) FOR [macroPropertySortOrder]
		;

		ALTER TABLE [cmsMember] ADD  CONSTRAINT [DF_cmsMember_Email]  DEFAULT ('') FOR [Email]
		;

		ALTER TABLE [cmsMember] ADD  CONSTRAINT [DF_cmsMember_LoginName]  DEFAULT ('') FOR [LoginName]
		;

		ALTER TABLE [cmsMember] ADD  CONSTRAINT [DF_cmsMember_Password]  DEFAULT ('') FOR [Password]
		;

		ALTER TABLE [cmsMemberType] ADD  CONSTRAINT [DF_cmsMemberType_memberCanEdit]  DEFAULT ((0)) FOR [memberCanEdit]
		;

		ALTER TABLE [cmsMemberType] ADD  CONSTRAINT [DF_cmsMemberType_viewOnProfile]  DEFAULT ((0)) FOR [viewOnProfile]
		;

		ALTER TABLE [cmsPropertyType] ADD  CONSTRAINT [DF__cmsProper__sortO__1EA48E88]  DEFAULT ((0)) FOR [sortOrder]
		;

		ALTER TABLE [cmsPropertyType] ADD  CONSTRAINT [DF__cmsProper__manda__2180FB33]  DEFAULT ((0)) FOR [mandatory]
		;

		ALTER TABLE [cmsTask] ADD  CONSTRAINT [DF__cmsTask__closed__04E4BC85]  DEFAULT ((0)) FOR [closed]
		;

		ALTER TABLE [cmsTask] ADD  CONSTRAINT [DF__cmsTask__DateTim__05D8E0BE]  DEFAULT (getdate()) FOR [DateTime]
		;

		ALTER TABLE [umbracoApp] ADD  CONSTRAINT [DF_app_sortOrder]  DEFAULT ((0)) FOR [sortOrder]
		;

		ALTER TABLE [umbracoAppTree] ADD  CONSTRAINT [DF_umbracoAppTree_treeSilent]  DEFAULT ((0)) FOR [treeSilent]
		;

		ALTER TABLE [umbracoAppTree] ADD  CONSTRAINT [DF_umbracoAppTree_treeInitialize]  DEFAULT ((1)) FOR [treeInitialize]
		;

		ALTER TABLE [umbracoLog] ADD  CONSTRAINT [DF_umbracoLog_Datestamp]  DEFAULT (getdate()) FOR [Datestamp]
		;

		ALTER TABLE [umbracoNode] ADD  CONSTRAINT [DF_umbracoNode_trashed]  DEFAULT ((0)) FOR [trashed]
		;

		ALTER TABLE [umbracoNode] ADD  CONSTRAINT [DF_umbracoNode_createDate]  DEFAULT (getdate()) FOR [createDate]
		;

		ALTER TABLE [umbracoRelation] ADD  CONSTRAINT [DF_umbracoRelation_datetime]  DEFAULT (getdate()) FOR [datetime]
		;

		ALTER TABLE [umbracoUser] ADD  CONSTRAINT [DF_umbracoUser_userDisabled]  DEFAULT ((0)) FOR [userDisabled]
		;

		ALTER TABLE [umbracoUser] ADD  CONSTRAINT [DF_umbracoUser_userNoConsole]  DEFAULT ((0)) FOR [userNoConsole]
		;

		ALTER TABLE [umbracoUser] ADD  CONSTRAINT [DF_umbracoUser_defaultToLiveEditing]  DEFAULT ((0)) FOR [defaultToLiveEditing]
		;

		ALTER TABLE [cmsContent]  WITH CHECK ADD  CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsContent] CHECK CONSTRAINT [FK_cmsContent_umbracoNode]
		;

		ALTER TABLE [cmsContentType]  WITH CHECK ADD  CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsContentType] CHECK CONSTRAINT [FK_cmsContentType_umbracoNode]
		;

		ALTER TABLE [cmsContentTypeAllowedContentType]  WITH CHECK ADD  CONSTRAINT [FK_cmsContentTypeAllowedContentType_cmsContentType] FOREIGN KEY([Id])
		REFERENCES [cmsContentType] ([nodeId])
		;
		ALTER TABLE [cmsContentTypeAllowedContentType] CHECK CONSTRAINT [FK_cmsContentTypeAllowedContentType_cmsContentType]
		;

		ALTER TABLE [cmsContentTypeAllowedContentType]  WITH CHECK ADD  CONSTRAINT [FK_cmsContentTypeAllowedContentType_cmsContentType1] FOREIGN KEY([AllowedId])
		REFERENCES [cmsContentType] ([nodeId])
		;
		ALTER TABLE [cmsContentTypeAllowedContentType] CHECK CONSTRAINT [FK_cmsContentTypeAllowedContentType_cmsContentType1]
		;

		ALTER TABLE [cmsContentVersion]  WITH CHECK ADD  CONSTRAINT [FK_cmsContentVersion_cmsContent] FOREIGN KEY([ContentId])
		REFERENCES [cmsContent] ([nodeId])
		;
		ALTER TABLE [cmsContentVersion] CHECK CONSTRAINT [FK_cmsContentVersion_cmsContent]
		;

		ALTER TABLE [cmsContentXml]  WITH CHECK ADD  CONSTRAINT [FK_cmsContentXml_cmsContent] FOREIGN KEY([nodeId])
		REFERENCES [cmsContent] ([nodeId])
		;
		ALTER TABLE [cmsContentXml] CHECK CONSTRAINT [FK_cmsContentXml_cmsContent]
		;

		ALTER TABLE [cmsDataTypePreValues]  WITH CHECK ADD  CONSTRAINT [FK_cmsDataTypePreValues_cmsDataType] FOREIGN KEY([datatypeNodeId])
		REFERENCES [cmsDataType] ([nodeId])
		;
		ALTER TABLE [cmsDataTypePreValues] CHECK CONSTRAINT [FK_cmsDataTypePreValues_cmsDataType]
		;

		ALTER TABLE [cmsDocument]  WITH CHECK ADD  CONSTRAINT [FK_cmsDocument_cmsContent] FOREIGN KEY([nodeId])
		REFERENCES [cmsContent] ([nodeId])
		;
		ALTER TABLE [cmsDocument] CHECK CONSTRAINT [FK_cmsDocument_cmsContent]
		;

		ALTER TABLE [cmsDocument]  WITH CHECK ADD  CONSTRAINT [FK_cmsDocument_cmsTemplate] FOREIGN KEY([templateId])
		REFERENCES [cmsTemplate] ([nodeId])
		;
		ALTER TABLE [cmsDocument] CHECK CONSTRAINT [FK_cmsDocument_cmsTemplate]
		;

		ALTER TABLE [cmsDocument]  WITH CHECK ADD  CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsDocument] CHECK CONSTRAINT [FK_cmsDocument_umbracoNode]
		;

		ALTER TABLE [cmsLanguageText]  WITH CHECK ADD  CONSTRAINT [FK_cmsLanguageText_cmsDictionary] FOREIGN KEY([UniqueId])
		REFERENCES [cmsDictionary] ([id])
		;
		ALTER TABLE [cmsLanguageText] CHECK CONSTRAINT [FK_cmsLanguageText_cmsDictionary]
		;

		ALTER TABLE [cmsMacroProperty]  WITH CHECK ADD  CONSTRAINT [FK_cmsMacroProperty_cmsMacro] FOREIGN KEY([macro])
		REFERENCES [cmsMacro] ([id])
		;
		ALTER TABLE [cmsMacroProperty] CHECK CONSTRAINT [FK_cmsMacroProperty_cmsMacro]
		;

		ALTER TABLE [cmsMacroProperty]  WITH CHECK ADD  CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY([macroPropertyType])
		REFERENCES [cmsMacroPropertyType] ([id])
		;
		ALTER TABLE [cmsMacroProperty] CHECK CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType]
		;

		ALTER TABLE [cmsMember]  WITH CHECK ADD  CONSTRAINT [FK_cmsMember_cmsContent] FOREIGN KEY([nodeId])
		REFERENCES [cmsContent] ([nodeId])
		;
		ALTER TABLE [cmsMember] CHECK CONSTRAINT [FK_cmsMember_cmsContent]
		;

		ALTER TABLE [cmsMember]  WITH CHECK ADD  CONSTRAINT [FK_cmsMember_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsMember] CHECK CONSTRAINT [FK_cmsMember_umbracoNode]
		;

		ALTER TABLE [cmsMember2MemberGroup]  WITH CHECK ADD  CONSTRAINT [FK_cmsMember2MemberGroup_cmsMember] FOREIGN KEY([Member])
		REFERENCES [cmsMember] ([nodeId])
		;
		ALTER TABLE [cmsMember2MemberGroup] CHECK CONSTRAINT [FK_cmsMember2MemberGroup_cmsMember]
		;

		ALTER TABLE [cmsMember2MemberGroup]  WITH CHECK ADD  CONSTRAINT [FK_cmsMember2MemberGroup_umbracoNode] FOREIGN KEY([MemberGroup])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsMember2MemberGroup] CHECK CONSTRAINT [FK_cmsMember2MemberGroup_umbracoNode]
		;

		ALTER TABLE [cmsMemberType]  WITH CHECK ADD  CONSTRAINT [FK_cmsMemberType_cmsContentType] FOREIGN KEY([NodeId])
		REFERENCES [cmsContentType] ([nodeId])
		;
		ALTER TABLE [cmsMemberType] CHECK CONSTRAINT [FK_cmsMemberType_cmsContentType]
		;

		ALTER TABLE [cmsMemberType]  WITH CHECK ADD  CONSTRAINT [FK_cmsMemberType_umbracoNode] FOREIGN KEY([NodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsMemberType] CHECK CONSTRAINT [FK_cmsMemberType_umbracoNode]
		;

		ALTER TABLE [cmsPreviewXml]  WITH CHECK ADD  CONSTRAINT [FK_cmsPreviewXml_cmsContent] FOREIGN KEY([nodeId])
		REFERENCES [cmsContent] ([nodeId])
		;
		ALTER TABLE [cmsPreviewXml] CHECK CONSTRAINT [FK_cmsPreviewXml_cmsContent]
		;

		ALTER TABLE [cmsPreviewXml]  WITH CHECK ADD  CONSTRAINT [FK_cmsPreviewXml_cmsContentVersion] FOREIGN KEY([versionId])
		REFERENCES [cmsContentVersion] ([VersionId])
		;
		ALTER TABLE [cmsPreviewXml] CHECK CONSTRAINT [FK_cmsPreviewXml_cmsContentVersion]
		;

		ALTER TABLE [cmsPropertyData]  WITH CHECK ADD  CONSTRAINT [FK_cmsPropertyData_cmsPropertyType] FOREIGN KEY([propertytypeid])
		REFERENCES [cmsPropertyType] ([id])
		;
		ALTER TABLE [cmsPropertyData] CHECK CONSTRAINT [FK_cmsPropertyData_cmsPropertyType]
		;

		ALTER TABLE [cmsPropertyData]  WITH CHECK ADD  CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY([contentNodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsPropertyData] CHECK CONSTRAINT [FK_cmsPropertyData_umbracoNode]
		;

		ALTER TABLE [cmsPropertyType]  WITH CHECK ADD  CONSTRAINT [FK_cmsPropertyType_cmsContentType] FOREIGN KEY([contentTypeId])
		REFERENCES [cmsContentType] ([nodeId])
		;
		ALTER TABLE [cmsPropertyType] CHECK CONSTRAINT [FK_cmsPropertyType_cmsContentType]
		;

		ALTER TABLE [cmsPropertyType]  WITH CHECK ADD  CONSTRAINT [FK_cmsPropertyType_cmsDataType] FOREIGN KEY([dataTypeId])
		REFERENCES [cmsDataType] ([nodeId])
		;
		ALTER TABLE [cmsPropertyType] CHECK CONSTRAINT [FK_cmsPropertyType_cmsDataType]
		;

		ALTER TABLE [cmsPropertyType]  WITH CHECK ADD  CONSTRAINT [FK_cmsPropertyType_cmsTab] FOREIGN KEY([tabId])
		REFERENCES [cmsTab] ([id])
		;
		ALTER TABLE [cmsPropertyType] CHECK CONSTRAINT [FK_cmsPropertyType_cmsTab]
		;

		ALTER TABLE [cmsStylesheet]  WITH CHECK ADD  CONSTRAINT [FK_cmsStylesheet_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsStylesheet] CHECK CONSTRAINT [FK_cmsStylesheet_umbracoNode]
		;

		ALTER TABLE [cmsStylesheetProperty]  WITH CHECK ADD  CONSTRAINT [FK_cmsStylesheetProperty_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsStylesheetProperty] CHECK CONSTRAINT [FK_cmsStylesheetProperty_umbracoNode]
		;

		ALTER TABLE [cmsTab]  WITH CHECK ADD  CONSTRAINT [FK_cmsTab_cmsContentType] FOREIGN KEY([contenttypeNodeId])
		REFERENCES [cmsContentType] ([nodeId])
		;
		ALTER TABLE [cmsTab] CHECK CONSTRAINT [FK_cmsTab_cmsContentType]
		;

		ALTER TABLE [cmsTagRelationship]  WITH CHECK ADD  CONSTRAINT [cmsTags_cmsTagRelationship] FOREIGN KEY([tagId])
		REFERENCES [cmsTags] ([id])
		ON DELETE CASCADE
		;
		ALTER TABLE [cmsTagRelationship] CHECK CONSTRAINT [cmsTags_cmsTagRelationship]
		;

		ALTER TABLE [cmsTagRelationship]  WITH CHECK ADD  CONSTRAINT [umbracoNode_cmsTagRelationship] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		ON DELETE CASCADE
		;
		ALTER TABLE [cmsTagRelationship] CHECK CONSTRAINT [umbracoNode_cmsTagRelationship]
		;

		ALTER TABLE [cmsTask]  WITH CHECK ADD  CONSTRAINT [FK_cmsTask_cmsTaskType] FOREIGN KEY([taskTypeId])
		REFERENCES [cmsTaskType] ([id])
		;
		ALTER TABLE [cmsTask] CHECK CONSTRAINT [FK_cmsTask_cmsTaskType]
		;

		ALTER TABLE [cmsTask]  WITH CHECK ADD  CONSTRAINT [FK_cmsTask_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsTask] CHECK CONSTRAINT [FK_cmsTask_umbracoNode]
		;

		ALTER TABLE [cmsTask]  WITH CHECK ADD  CONSTRAINT [FK_cmsTask_umbracoUser] FOREIGN KEY([parentUserId])
		REFERENCES [umbracoUser] ([id])
		;
		ALTER TABLE [cmsTask] CHECK CONSTRAINT [FK_cmsTask_umbracoUser]
		;

		ALTER TABLE [cmsTask]  WITH CHECK ADD  CONSTRAINT [FK_cmsTask_umbracoUser1] FOREIGN KEY([userId])
		REFERENCES [umbracoUser] ([id])
		;
		ALTER TABLE [cmsTask] CHECK CONSTRAINT [FK_cmsTask_umbracoUser1]
		;

		ALTER TABLE [cmsTemplate]  WITH CHECK ADD  CONSTRAINT [FK_cmsTemplate_cmsTemplate] FOREIGN KEY([master])
		REFERENCES [cmsTemplate] ([nodeId])
		;
		ALTER TABLE [cmsTemplate] CHECK CONSTRAINT [FK_cmsTemplate_cmsTemplate]
		;

		ALTER TABLE [cmsTemplate]  WITH CHECK ADD  CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [cmsTemplate] CHECK CONSTRAINT [FK_cmsTemplate_umbracoNode]
		;
/*
		ALTER TABLE [umbracoAppTree]  WITH CHECK ADD  CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY([appAlias])
		REFERENCES [umbracoApp] ([appAlias])
		;
		ALTER TABLE [umbracoAppTree] CHECK CONSTRAINT [FK_umbracoAppTree_umbracoApp]
		;
*/
		ALTER TABLE [umbracoDomains]  WITH CHECK ADD  CONSTRAINT [FK_umbracoDomains_umbracoNode] FOREIGN KEY([domainRootStructureID])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoDomains] CHECK CONSTRAINT [FK_umbracoDomains_umbracoNode]
		;

		ALTER TABLE [umbracoNode]  WITH CHECK ADD  CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY([parentID])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoNode] CHECK CONSTRAINT [FK_umbracoNode_umbracoNode]
		;

		ALTER TABLE [umbracoRelation]  WITH CHECK ADD  CONSTRAINT [FK_umbracoRelation_umbracoNode] FOREIGN KEY([parentId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoRelation] CHECK CONSTRAINT [FK_umbracoRelation_umbracoNode]
		;

		ALTER TABLE [umbracoRelation]  WITH CHECK ADD  CONSTRAINT [FK_umbracoRelation_umbracoNode1] FOREIGN KEY([childId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoRelation] CHECK CONSTRAINT [FK_umbracoRelation_umbracoNode1]
		;

		ALTER TABLE [umbracoRelation]  WITH CHECK ADD  CONSTRAINT [FK_umbracoRelation_umbracoRelationType] FOREIGN KEY([relType])
		REFERENCES [umbracoRelationType] ([id])
		;
		ALTER TABLE [umbracoRelation] CHECK CONSTRAINT [FK_umbracoRelation_umbracoRelationType]
		;

		ALTER TABLE [umbracoUser]  WITH CHECK ADD  CONSTRAINT [FK_user_userType] FOREIGN KEY([userType])
		REFERENCES [umbracoUserType] ([id])
		;
		ALTER TABLE [umbracoUser] CHECK CONSTRAINT [FK_user_userType]
		;
/*
		ALTER TABLE [umbracoUser2app]  WITH CHECK ADD  CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY([app])
		REFERENCES [umbracoApp] ([appAlias])
		;
		ALTER TABLE [umbracoUser2app] CHECK CONSTRAINT [FK_umbracoUser2app_umbracoApp]
		;
*/
		ALTER TABLE [umbracoUser2NodeNotify]  WITH CHECK ADD  CONSTRAINT [FK_umbracoUser2NodeNotify_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoUser2NodeNotify] CHECK CONSTRAINT [FK_umbracoUser2NodeNotify_umbracoNode]
		;

		ALTER TABLE [umbracoUser2NodeNotify]  WITH CHECK ADD  CONSTRAINT [FK_umbracoUser2NodeNotify_umbracoUser] FOREIGN KEY([userId])
		REFERENCES [umbracoUser] ([id])
		;
		ALTER TABLE [umbracoUser2NodeNotify] CHECK CONSTRAINT [FK_umbracoUser2NodeNotify_umbracoUser]
		;

		ALTER TABLE [umbracoUser2NodePermission]  WITH CHECK ADD  CONSTRAINT [FK_umbracoUser2NodePermission_umbracoNode] FOREIGN KEY([nodeId])
		REFERENCES [umbracoNode] ([id])
		;
		ALTER TABLE [umbracoUser2NodePermission] CHECK CONSTRAINT [FK_umbracoUser2NodePermission_umbracoNode]
		;

		ALTER TABLE [umbracoUser2NodePermission]  WITH CHECK ADD  CONSTRAINT [FK_umbracoUser2NodePermission_umbracoUser] FOREIGN KEY([userId])
		REFERENCES [umbracoUser] ([id])
		;
		ALTER TABLE [umbracoUser2NodePermission] CHECK CONSTRAINT [FK_umbracoUser2NodePermission_umbracoUser]
		;
END