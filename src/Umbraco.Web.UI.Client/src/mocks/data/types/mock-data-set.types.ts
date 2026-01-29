import type {
	// Data Type
	DataTypeItemResponseModel,
	DataTypeResponseModel,
	DataTypeTreeItemResponseModel,
	// Dictionary
	DictionaryItemItemResponseModel,
	DictionaryItemResponseModel,
	DictionaryOverviewResponseModel,
	NamedEntityTreeItemResponseModel,
	// Document
	DocumentItemResponseModel,
	DocumentResponseModel,
	DocumentTreeItemResponseModel,
	// Document Blueprint
	DocumentBlueprintItemResponseModel,
	DocumentBlueprintResponseModel,
	DocumentBlueprintTreeItemResponseModel,
	// Document Type
	DocumentTypeItemResponseModel,
	DocumentTypeResponseModel,
	DocumentTypeTreeItemResponseModel,
	// Language
	LanguageItemResponseModel,
	LanguageResponseModel,
	// Media
	MediaItemResponseModel,
	MediaResponseModel,
	MediaTreeItemResponseModel,
	// Media Type
	MediaTypeItemResponseModel,
	MediaTypeResponseModel,
	MediaTypeTreeItemResponseModel,
	// Member
	MemberItemResponseModel,
	MemberResponseModel,
	// Member Group
	MemberGroupItemResponseModel,
	// Member Type
	MemberTypeItemResponseModel,
	MemberTypeResponseModel,
	// Partial View
	FileSystemTreeItemPresentationModel,
	PartialViewItemResponseModel,
	PartialViewResponseModel,
	PartialViewSnippetResponseModel,
	// Relation
	RelationResponseModel,
	// Relation Type
	RelationTypeItemResponseModel,
	RelationTypeResponseModel,
	// Script
	ScriptItemResponseModel,
	ScriptResponseModel,
	// Static File
	StaticFileItemResponseModel,
	// Stylesheet
	StylesheetItemResponseModel,
	StylesheetResponseModel,
	// Template
	TemplateItemResponseModel,
	TemplateResponseModel,
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
	// User
	UserItemResponseModel,
	UserResponseModel,
	UserTwoFactorProviderModel,
	// User Group
	UserGroupItemResponseModel,
	UserGroupResponseModel,
	// Object Type
	ObjectTypeResponseModel,
	// Log Viewer
	LogTemplateResponseModel,
	SavedLogSearchResponseModel,
	// Logs
	LogMessageResponseModel,
	// Audit Log
	AuditLogResponseModel,
	// Health Check
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
	// Examine
	IndexResponseModel,
	PagedIndexResponseModel,
	SearchResultResponseModel,
	// Tracked Reference
	DefaultReferenceResponseModel,
	DocumentReferenceResponseModel,
	MediaReferenceResponseModel,
	MemberReferenceResponseModel,
	// News
	NewsDashboardItemResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

// ============================================================================
// Model Type Definitions
// ============================================================================

export type UmbMockDataTypeModel = DataTypeResponseModel & DataTypeTreeItemResponseModel & DataTypeItemResponseModel;

export type UmbMockDictionaryModel = DictionaryItemResponseModel &
	NamedEntityTreeItemResponseModel &
	DictionaryItemItemResponseModel &
	DictionaryOverviewResponseModel;

export type UmbMockDocumentModel = DocumentResponseModel & DocumentTreeItemResponseModel & DocumentItemResponseModel;

export type UmbMockDocumentBlueprintModel = DocumentBlueprintResponseModel &
	DocumentBlueprintItemResponseModel &
	DocumentBlueprintTreeItemResponseModel;

export type UmbMockDocumentTypeModel = DocumentTypeResponseModel &
	DocumentTypeTreeItemResponseModel &
	DocumentTypeItemResponseModel;

export type UmbMockLanguageModel = LanguageResponseModel & LanguageItemResponseModel;

export type UmbMockMediaModel = MediaResponseModel & MediaTreeItemResponseModel & MediaItemResponseModel;

export type UmbMockMediaTypeModel = MediaTypeResponseModel &
	MediaTypeTreeItemResponseModel &
	MediaTypeItemResponseModel;

export type UmbMockMemberModel = MemberResponseModel & MemberItemResponseModel;

export type UmbMockMemberGroupModel = MemberGroupItemResponseModel;

export type UmbMockMemberTypeModel = MemberTypeResponseModel &
	MemberTypeItemResponseModel & {
		hasChildren: boolean;
		parent: { id: string } | null;
		hasListView: boolean;
	};

export type UmbMockPartialViewModel = PartialViewResponseModel &
	FileSystemTreeItemPresentationModel &
	PartialViewItemResponseModel;

export type UmbMockRelationModel = RelationResponseModel;

export type UmbMockRelationTypeModel = RelationTypeResponseModel & RelationTypeItemResponseModel;

export type UmbMockRelationTypeItemModel = RelationTypeItemResponseModel;

export type UmbMockScriptModel = ScriptResponseModel & FileSystemTreeItemPresentationModel & ScriptItemResponseModel;

export type UmbMockStaticFileModel = StaticFileItemResponseModel & FileSystemTreeItemPresentationModel;

export type UmbMockStylesheetModel = StylesheetResponseModel &
	FileSystemTreeItemPresentationModel &
	StylesheetItemResponseModel;

export type UmbMockTemplateModel = TemplateResponseModel & NamedEntityTreeItemResponseModel & TemplateItemResponseModel;

export type UmbMockUserModel = UserResponseModel & UserItemResponseModel;

export type UmbMockUserGroupModel = UserGroupResponseModel & UserGroupItemResponseModel;

export type UmbMockTrackedReferenceItemModel =
	| DefaultReferenceResponseModel
	| DocumentReferenceResponseModel
	| MediaReferenceResponseModel
	| MemberReferenceResponseModel;

// ============================================================================
// Log Levels Type (matches the structure in log-viewer.data.ts)
// ============================================================================

export interface UmbMockLogLevelsModel {
	total: number;
	items: Array<{
		name: string;
		level: string;
	}>;
}

// ============================================================================
// Mock Data Set Interface
// ============================================================================

/**
 * Interface describing the complete structure of a mock data set.
 * All data sets (default, test, etc.) must implement this interface.
 */
export interface UmbMockDataSet {
	// Core entity data arrays
	dataType: Array<UmbMockDataTypeModel>;
	dictionary: Array<UmbMockDictionaryModel>;
	document: Array<UmbMockDocumentModel>;
	documentBlueprint: Array<UmbMockDocumentBlueprintModel>;
	documentType: Array<UmbMockDocumentTypeModel>;
	language: Array<UmbMockLanguageModel>;
	media: Array<UmbMockMediaModel>;
	mediaType: Array<UmbMockMediaTypeModel>;
	member: Array<UmbMockMemberModel>;
	memberGroup: Array<UmbMockMemberGroupModel>;
	memberType: Array<UmbMockMemberTypeModel>;
	partialView: Array<UmbMockPartialViewModel>;
	partialViewSnippets: Array<PartialViewSnippetResponseModel>;
	relation: Array<UmbMockRelationModel>;
	relationType: Array<UmbMockRelationTypeModel>;
	script: Array<UmbMockScriptModel>;
	staticFile: Array<UmbMockStaticFileModel>;
	stylesheet: Array<UmbMockStylesheetModel>;
	template: Array<UmbMockTemplateModel>;
	user: Array<UmbMockUserModel>;
	userGroup: Array<UmbMockUserGroupModel>;
	objectType: Array<ObjectTypeResponseModel>;

	// Log viewer data
	logViewerSavedSearches: Array<SavedLogSearchResponseModel>;
	logViewerMessageTemplates: Array<LogTemplateResponseModel>;
	logViewerLogLevels: UmbMockLogLevelsModel;
	logs: Array<LogMessageResponseModel>;

	// Audit logs
	auditLogs: Array<AuditLogResponseModel>;

	// Health check data
	healthGroups: Array<HealthCheckGroupWithResultResponseModel & { name: string }>;
	healthGroupsWithoutResult: Array<HealthCheckGroupPresentationModel>;

	// Examine/search data
	examineIndexers: Array<IndexResponseModel>;
	examinePagedIndexers: PagedIndexResponseModel;
	examineSearchResults: Array<SearchResultResponseModel>;
	examineGetSearchResults: () => Array<SearchResultResponseModel>;

	// Tracked references
	trackedReferenceItems: Array<UmbMockTrackedReferenceItemModel>;

	// News
	news: Array<NewsDashboardItemResponseModel>;

	// User-specific extras
	mfaLoginProviders: Array<UserTwoFactorProviderModel>;

	// Template-specific helpers
	templateQueryResult: TemplateQueryResultResponseModel;
	templateQuerySettings: TemplateQuerySettingsResponseModel;
}

// ============================================================================
// Type-safe data key mapping
// ============================================================================

/**
 * Maps string keys to their corresponding data types for type-safe getMockData()
 */
export type UmbMockDataKeyMap = {
	[K in keyof UmbMockDataSet]: UmbMockDataSet[K];
};

export type UmbMockDataKey = keyof UmbMockDataSet;
