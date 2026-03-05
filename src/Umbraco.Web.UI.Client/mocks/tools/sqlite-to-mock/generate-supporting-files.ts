/**
 * Generate supporting files for a mock data set.
 * These are files that don't come from the SQLite database but are required
 * for a complete mock data set (index.ts, placeholder files, static data).
 */
import { writeDataFile, getOutputDir } from './db.js';
import path from 'path';
import fs from 'fs';

/**
 * Generate all supporting files for a mock data set
 */
export function generateSupportingFiles(setAlias: string): void {
	const outputDir = getOutputDir();

	// Generate index.ts
	generateIndexFile(setAlias);

	// Generate placeholder files (empty arrays)
	generatePlaceholderFiles();

	// Generate static data files
	generateStaticDataFiles(setAlias);

	// Generate files that depend on generated data
	generateAuditLogFile();

	console.log('Generated supporting files');
}

function generateIndexFile(setAlias: string): void {
	const capitalizedAlias = setAlias.charAt(0).toUpperCase() + setAlias.slice(1);

	const content = `// ${capitalizedAlias} mock data set - implements UmbMockDataSet interface
// Generated from Umbraco SQLite database export
import type { UmbMockDataSet } from '../../types/mock-data-set.types.js';

// Import all data
import { data as dataType } from './data-type.data.js';
import { data as dictionary } from './dictionary.data.js';
import { data as document } from './document.data.js';
import { data as documentBlueprint } from './document-blueprint.data.js';
import { data as documentType } from './document-type.data.js';
import { data as language } from './language.data.js';
import { data as media } from './media.data.js';
import { data as mediaType } from './media-type.data.js';
import { data as member } from './member.data.js';
import { data as memberGroup } from './member-group.data.js';
import { data as memberType } from './member-type.data.js';
import { data as partialView, snippets as partialViewSnippets } from './partial-view.data.js';
import { data as relation } from './relation.data.js';
import { data as relationType } from './relationType.data.js';
import { data as script } from './script.data.js';
import { data as staticFile } from './static-file.data.js';
import { data as stylesheet } from './stylesheet.data.js';
import { data as template, templateQueryResult, templateQuerySettings } from './template.data.js';
import { data as user } from './user.data.js';
import { data as mfaLoginProviders } from './user-mfa-login-providers.data.js';
import { data as userGroup } from './user-group.data.js';
import { data as objectType } from './object-type.data.js';
import {
	savedSearches as logViewerSavedSearches,
	messageTemplates as logViewerMessageTemplates,
	logLevels as logViewerLogLevels,
} from './log-viewer.data.js';
import { data as logs } from './logs.data.js';
import { data as auditLogs } from './audit-log.data.js';
import { healthGroups, healthGroupsWithoutResult } from './health-check.data.js';
import {
	Indexers as examineIndexers,
	PagedIndexers as examinePagedIndexers,
	searchResultMockData as examineSearchResults,
} from './examine.data.js';
import { items as trackedReferenceItems } from './tracked-reference.data.js';
import { data as news } from './umbraco-news.data.js';

// Named exports for all data
export {
	dataType,
	dictionary,
	document,
	documentBlueprint,
	documentType,
	language,
	media,
	mediaType,
	member,
	memberGroup,
	memberType,
	partialView,
	partialViewSnippets,
	relation,
	relationType,
	script,
	staticFile,
	stylesheet,
	template,
	templateQueryResult,
	templateQuerySettings,
	user,
	mfaLoginProviders,
	userGroup,
	objectType,
	logViewerSavedSearches,
	logViewerMessageTemplates,
	logViewerLogLevels,
	logs,
	auditLogs,
	healthGroups,
	healthGroupsWithoutResult,
	examineIndexers,
	examinePagedIndexers,
	examineSearchResults,
	trackedReferenceItems,
	news,
};

// Type assertion to ensure this module satisfies UmbMockDataSet
const _typeCheck: UmbMockDataSet = {
	dataType,
	dictionary,
	document,
	documentBlueprint,
	documentType,
	language,
	media,
	mediaType,
	member,
	memberGroup,
	memberType,
	partialView,
	partialViewSnippets,
	relation,
	relationType,
	script,
	staticFile,
	stylesheet,
	template,
	templateQueryResult,
	templateQuerySettings,
	user,
	mfaLoginProviders,
	userGroup,
	objectType,
	logViewerSavedSearches,
	logViewerMessageTemplates,
	logViewerLogLevels,
	logs,
	auditLogs,
	healthGroups,
	healthGroupsWithoutResult,
	examineIndexers,
	examinePagedIndexers,
	examineSearchResults,
	trackedReferenceItems,
	news,
};
void _typeCheck;
`;

	const outputDir = getOutputDir();
	const outputPath = path.join(outputDir, 'index.ts');
	fs.writeFileSync(outputPath, content, 'utf-8');
	console.log('Written: index.ts');
}

function generatePlaceholderFiles(): void {
	// Document blueprints
	writeDataFile(
		'document-blueprint.data.ts',
		`import type { UmbMockDocumentBlueprintModel } from '../../types/mock-data-set.types.js';

// Empty document blueprints - no blueprints in the database
export const data: Array<UmbMockDocumentBlueprintModel> = [];
`,
	);

	// Members
	writeDataFile(
		'member.data.ts',
		`import type { UmbMockMemberModel } from '../../types/mock-data-set.types.js';

// Empty members - no members in the database
export const data: Array<UmbMockMemberModel> = [];
`,
	);

	// Member groups
	writeDataFile(
		'member-group.data.ts',
		`import type { UmbMockMemberGroupModel } from '../../types/mock-data-set.types.js';

// Empty member groups - no member groups in the database
export const data: Array<UmbMockMemberGroupModel> = [];
`,
	);

	// Member types
	writeDataFile(
		'member-type.data.ts',
		`import type { UmbMockMemberTypeModel } from '../../types/mock-data-set.types.js';

// Empty member types - no member types in the database
export const data: Array<UmbMockMemberTypeModel> = [];
`,
	);

	// Relations
	writeDataFile(
		'relation.data.ts',
		`import type { UmbMockRelationModel } from '../../types/mock-data-set.types.js';

// Empty relations
export const data: Array<UmbMockRelationModel> = [];
`,
	);

	// Relation types
	writeDataFile(
		'relationType.data.ts',
		`import type { UmbMockRelationTypeModel } from '../../types/mock-data-set.types.js';

// Empty relation types
export const data: Array<UmbMockRelationTypeModel> = [];
`,
	);

	// Scripts
	writeDataFile(
		'script.data.ts',
		`import type { UmbMockScriptModel } from '../../types/mock-data-set.types.js';

// Empty scripts
export const data: Array<UmbMockScriptModel> = [];
`,
	);

	// Static files
	writeDataFile(
		'static-file.data.ts',
		`import type { UmbMockStaticFileModel } from '../../types/mock-data-set.types.js';

// Empty static files
export const data: Array<UmbMockStaticFileModel> = [];
`,
	);

	// Stylesheets
	writeDataFile(
		'stylesheet.data.ts',
		`import type { UmbMockStylesheetModel } from '../../types/mock-data-set.types.js';

// Empty stylesheets
export const data: Array<UmbMockStylesheetModel> = [];
`,
	);

	// Tracked references
	writeDataFile(
		'tracked-reference.data.ts',
		`import type { UmbMockTrackedReferenceItemModel } from '../../types/mock-data-set.types.js';

// Empty tracked references
export const items: Array<UmbMockTrackedReferenceItemModel> = [];
`,
	);
}

function generateStaticDataFiles(setAlias: string): void {
	const capitalizedAlias = setAlias.charAt(0).toUpperCase() + setAlias.slice(1);

	// Object types
	writeDataFile(
		'object-type.data.ts',
		`import type { ObjectTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<ObjectTypeResponseModel> = [
	{
		id: '1',
		name: 'Media',
	},
	{
		id: '2',
		name: 'Content',
	},
	{
		id: '3',
		name: 'User',
	},
	{
		id: '4',
		name: 'Document',
	},
];
`,
	);

	// User MFA login providers
	writeDataFile(
		'user-mfa-login-providers.data.ts',
		`import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Mock data for MFA login providers
 */
export const data: Array<UserTwoFactorProviderModel> = [
	{
		isEnabledOnUser: false,
		providerName: 'Google Authenticator',
	},
];
`,
	);

	// Partial views
	writeDataFile(
		'partial-view.data.ts',
		`import type { UmbMockPartialViewModel } from '../../types/mock-data-set.types.js';
import type { PartialViewSnippetResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<UmbMockPartialViewModel> = [
	{
		name: 'blockgrid',
		path: '/blockgrid',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'blocklist',
		path: '/blocklist',
		parent: null,
		isFolder: true,
		hasChildren: true,
		content: '',
	},
	{
		name: 'area.cshtml',
		path: '/blockgrid/area.cshtml',
		parent: {
			path: '/blockgrid',
		},
		isFolder: false,
		hasChildren: false,
		content: \`@using Umbraco.Extensions
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridArea>

<div class="umb-block-grid__area"
	 data-area-col-span="@Model.ColumnSpan"
	 data-area-row-span="@Model.RowSpan"
	 data-area-alias="@Model.Alias"
	 style="--umb-block-grid--grid-columns: @Model.ColumnSpan;--umb-block-grid--area-column-span: @Model.ColumnSpan; --umb-block-grid--area-row-span: @Model.RowSpan;">
	@await Html.GetBlockGridItemsHtmlAsync(Model)
</div>\`,
	},
	{
		name: 'default.cshtml',
		path: '/blocklist/default.cshtml',
		parent: {
			path: '/blocklist',
		},
		isFolder: false,
		hasChildren: false,
		content: \`@using Umbraco.Extensions
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<Umbraco.Cms.Core.Models.Blocks.BlockGridModel>
@{
	if (Model?.Any() != true) { return; }
}

<div class="umb-block-grid"
	 data-grid-columns="@(Model.GridColumns?.ToString() ?? "12");"
	 style="--umb-block-grid--grid-columns: @(Model.GridColumns?.ToString() ?? "12");">
	@await Html.GetBlockGridItemsHtmlAsync(Model)
</div>\`,
	},
];

export const snippets: Array<PartialViewSnippetResponseModel> = [
	{
		name: 'Empty',
		id: '37f8786b-0b9b-466f-97b6-e736126fc545',
		content: '@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage',
	},
	{
		name: 'Breadcrumb',
		id: '4ed59952-d0aa-4583-9c3d-9f6b7068dcea',
		content: \`@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage
@using Umbraco.Cms.Core.Routing
@using Umbraco.Extensions

@inject IPublishedUrlProvider PublishedUrlProvider

@{ var selection = Model.Ancestors().ToArray(); }

@if (selection?.Length > 0)
{
	<ul class="breadcrumb">
		@foreach (var item in selection.OrderBy(x => x.Level))
		{
			<li><a href="@item.Url(PublishedUrlProvider)">@item.Name</a> <span class="divider">/</span></li>
		}
		<li class="active">@Model.Name</li>
	</ul>
}\`,
	},
];
`,
	);

	// Logs
	writeDataFile(
		'logs.data.ts',
		`import type { LogMessageResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { LogLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

// Minimal logs data
export const data: Array<LogMessageResponseModel> = [
	{
		timestamp: new Date().toISOString(),
		level: LogLevelModel.INFORMATION,
		messageTemplate: 'Application started',
		renderedMessage: 'Application started',
		properties: [],
		exception: null,
	},
];
`,
	);

	// Log viewer
	writeDataFile(
		'log-viewer.data.ts',
		`import type {
	LogTemplateResponseModel,
	SavedLogSearchResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const savedSearches: Array<SavedLogSearchResponseModel> = [
	{
		name: 'Find all logs where the Level is NOT Verbose and NOT Debug',
		query: "Not(@Level='Verbose') and Not(@Level='Debug')",
	},
	{
		name: 'Find all logs that has an exception property (Warning, Error & Fatal with Exceptions)',
		query: 'Has(@Exception)',
	},
	{
		name: "Find all logs that have the property 'Duration'",
		query: 'Has(Duration)',
	},
	{
		name: "Find all logs that have the property 'Duration' and the duration is greater than 1000ms",
		query: 'Has(Duration) and Duration > 1000',
	},
	{
		name: "Find all logs that are from the namespace 'Umbraco.Core'",
		query: "StartsWith(SourceContext, 'Umbraco.Core')",
	},
];

export const messageTemplates: LogTemplateResponseModel[] = [
	{
		messageTemplate: 'Create Foreign Key:\\n {Sql}',
		count: 90,
	},
	{
		messageTemplate: 'Create Index:\\n {Sql}',
		count: 86,
	},
	{
		messageTemplate: 'Create table:\\n {Sql}',
		count: 82,
	},
	{
		messageTemplate: 'Create Primary Key:\\n {Sql}',
		count: 78,
	},
	{
		messageTemplate: 'Creating data in {TableName}',
		count: 58,
	},
];

export const logLevels = {
	total: 2,
	items: [
		{
			name: 'Global',
			level: 'Information',
		},
		{
			name: 'UmbracoFile',
			level: 'Verbose',
		},
	],
};
`,
	);

	// Umbraco news
	writeDataFile(
		'umbraco-news.data.ts',
		`import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<NewsDashboardItemResponseModel> = [
	{
		priority: 'High',
		header: 'Welcome to the ${capitalizedAlias} Mock Data Set!',
		body: \`
			<p>This mock data set was generated from a real Umbraco SQLite database export. It contains actual document types, media types, content, and more.</p>
			<strong>Note:</strong> This is a preview version of the Umbraco Backoffice using the ${setAlias} data set.
		\`,
		buttonText: 'Read more about Umbraco CMS',
		imageUrl: '',
		imageAltText: '',
		url: 'https://umbraco.com/products/umbraco-cms/',
	},
];
`,
	);

	// Health check
	writeDataFile(
		'health-check.data.ts',
		`import type {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export const healthGroups: Array<HealthCheckGroupWithResultResponseModel & { name: string }> = [
	{
		name: 'Configuration',
		checks: [
			{
				id: '3e2f7b14-4b41-452b-9a30-e67fbc8e1206',
				results: [
					{
						message: \`Notification email is still set to the default value of <strong>your@email.here</strong>.\`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-notification-email',
					},
				],
			},
		],
	},
	{
		name: 'Data Integrity',
		checks: [
			{
				id: '73dd0c1c-e0ca-4c31-9564-1dca509788af',
				results: [
					{
						message: \`All document paths are valid\`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{ message: \`All media paths are valid\`, resultType: StatusResultTypeModel.SUCCESS },
				],
			},
		],
	},
	{
		name: 'Live Environment',
		checks: [
			{
				id: '61214ff3-fc57-4b31-b5cf-1d095c977d6d',
				results: [
					{
						message: \`Debug compilation mode is currently enabled. It is recommended to disable this setting before go live.\`,
						resultType: StatusResultTypeModel.ERROR,
						readMoreLink: 'https://umbra.co/healthchecks-compilation-debug',
					},
				],
			},
		],
	},
	{
		name: 'Permissions',
		checks: [
			{
				id: '53dba282-4a79-4b67-b958-b29ec40fcc23',
				results: [
					{
						message: \`Folder creation\`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: \`File writing for packages\`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: \`File writing\`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
					{
						message: \`Media folder creation\`,
						resultType: StatusResultTypeModel.SUCCESS,
					},
				],
			},
		],
	},
	{
		name: 'Security',
		checks: [
			{
				id: '6708ca45-e96e-40b8-a40a-0607c1ca7f28',
				results: [
					{
						message: \`The appSetting 'Umbraco:CMS:WebRouting:UmbracoApplicationUrl' is not set\`,
						resultType: StatusResultTypeModel.WARNING,
						readMoreLink: 'https://umbra.co/healthchecks-umbraco-application-url',
					},
				],
			},
		],
	},
	{
		name: 'Services',
		checks: [
			{
				id: '1b5d221b-ce99-4193-97cb-5f3261ec73df',
				results: [
					{
						message: \`The 'Umbraco:CMS:Global:Smtp' configuration could not be found.\`,
						readMoreLink: 'https://umbra.co/healthchecks-smtp',
						resultType: StatusResultTypeModel.ERROR,
					},
				],
			},
		],
	},
];

export const healthGroupsWithoutResult: HealthCheckGroupPresentationModel[] = [
	{
		name: 'Configuration',
		checks: [
			{
				id: '3e2f7b14-4b41-452b-9a30-e67fbc8e1206',
				name: 'Notification Email Settings',
				description:
					"If notifications are used, the 'from' email address should be specified and changed from the default value.",
			},
		],
	},
	{
		name: 'Data Integrity',
		checks: [
			{
				id: '73dd0c1c-e0ca-4c31-9564-1dca509788af',
				name: 'Database data integrity check',
				description: 'Checks for various data integrity issues in the Umbraco database.',
			},
		],
	},
	{
		name: 'Live Environment',
		checks: [
			{
				id: '61214ff3-fc57-4b31-b5cf-1d095c977d6d',
				name: 'Debug Compilation Mode',
				description:
					'Leaving debug compilation mode enabled can severely slow down a website and take up more memory on the server.',
			},
		],
	},
	{
		name: 'Permissions',
		checks: [
			{
				id: '53dba282-4a79-4b67-b958-b29ec40fcc23',
				name: 'Folder & File Permissions',
				description: 'Checks that the web server folder and file permissions are set correctly for Umbraco to run.',
			},
		],
	},
	{
		name: 'Security',
		checks: [
			{
				id: '6708ca45-e96e-40b8-a40a-0607c1ca7f28',
				name: 'Application URL Configuration',
				description: 'Checks if the Umbraco application URL is configured for your site.',
			},
		],
	},
	{
		name: 'Services',
		checks: [
			{
				id: '1b5d221b-ce99-4193-97cb-5f3261ec73df',
				name: 'SMTP Settings',
				description: 'Checks that valid settings for sending emails are in place.',
			},
		],
	},
];
`,
	);

	// Examine
	writeDataFile(
		'examine.data.ts',
		`import type {
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { HealthStatusModel } from '@umbraco-cms/backoffice/external/backend-api';

export const Indexers: IndexResponseModel[] = [
	{
		name: 'ExternalIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		documentCount: 0,
		fieldCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'StandardAnalyzer',
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /externalindex',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			PublishedValuesOnly: true,
			SupportProtectedContent: false,
		},
	},
	{
		name: 'InternalIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		documentCount: 0,
		fieldCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /internalindex',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			PublishedValuesOnly: false,
			SupportProtectedContent: true,
			IncludeFields: ['id', 'nodeName', 'updateDate', 'loginName', 'email', '__Key'],
		},
	},
	{
		name: 'MemberIndex',
		canRebuild: true,
		healthStatus: { status: HealthStatusModel.HEALTHY },
		fieldCount: 0,
		documentCount: 0,
		searcherName: '',
		providerProperties: {
			CommitCount: 0,
			DefaultAnalyzer: 'CultureInvariantWhitespaceAnalyzer',
			DirectoryFactory:
				'Umbraco.Cms.Infrastructure.Examine.ConfigurationEnabledDirectoryFactory, Umbraco.Examine.Lucene, Version=10.2.0.0, Culture=neutral, PublicKeyToken=null',
			EnableDefaultEventHandler: true,
			IncludeFields: ['id', 'nodeName', 'updateDate', 'loginName', 'email', '__Key'],
			LuceneDirectory: 'SimpleFSDirectory',
			LuceneIndexFolder: '/ /umbraco /data /temp /examineindexes /membersindex',
			PublishedValuesOnly: false,
			SupportProtectedContent: false,
		},
	},
];

export const PagedIndexers: PagedIndexResponseModel = {
	items: Indexers,
	total: 0,
};

export const searchResultMockData: SearchResultResponseModel[] = [];
`,
	);
}

function generateAuditLogFile(): void {
	writeDataFile(
		'audit-log.data.ts',
		`import { data as userData } from './user.data.js';
import type { AuditLogResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { AuditTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

const userId = userData[0]?.id ?? '00000000-0000-0000-0000-000000000000';

export const data: Array<AuditLogResponseModel> = [
	{
		user: { id: userId },
		timestamp: '2021-09-14T09:32:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2022-09-14T11:30:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2022-09-15T09:35:49.0000000Z',
		logType: AuditTypeModel.SAVE,
		comment: null,
		parameters: null,
	},
	{
		user: { id: userId },
		timestamp: '2023-01-09T12:00:00.0000000Z',
		logType: AuditTypeModel.PUBLISH,
		comment: null,
		parameters: null,
	},
];
`,
	);
}
