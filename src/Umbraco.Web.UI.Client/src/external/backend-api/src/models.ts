

export type AllowedDocumentTypeModel = {
        id: string
name: string
description?: string | null
icon?: string | null
    };

export type AllowedMediaTypeModel = {
        id: string
name: string
description?: string | null
icon?: string | null
    };

export type AuditLogResponseModel = {
        user: ReferenceByIdModel
timestamp: string
logType: AuditTypeModel
comment?: string | null
parameters?: string | null
    };

export enum AuditTypeModel {
    NEW = 'New',
    SAVE = 'Save',
    SAVE_VARIANT = 'SaveVariant',
    OPEN = 'Open',
    DELETE = 'Delete',
    PUBLISH = 'Publish',
    PUBLISH_VARIANT = 'PublishVariant',
    SEND_TO_PUBLISH = 'SendToPublish',
    SEND_TO_PUBLISH_VARIANT = 'SendToPublishVariant',
    UNPUBLISH = 'Unpublish',
    UNPUBLISH_VARIANT = 'UnpublishVariant',
    MOVE = 'Move',
    COPY = 'Copy',
    ASSIGN_DOMAIN = 'AssignDomain',
    PUBLIC_ACCESS = 'PublicAccess',
    SORT = 'Sort',
    NOTIFY = 'Notify',
    SYSTEM = 'System',
    ROLL_BACK = 'RollBack',
    PACKAGER_INSTALL = 'PackagerInstall',
    PACKAGER_UNINSTALL = 'PackagerUninstall',
    CUSTOM = 'Custom',
    CONTENT_VERSION_PREVENT_CLEANUP = 'ContentVersionPreventCleanup',
    CONTENT_VERSION_ENABLE_CLEANUP = 'ContentVersionEnableCleanup'
}

export type AvailableDocumentTypeCompositionResponseModel = {
        id: string
name: string
icon: string
folderPath: Array<string>
isCompatible: boolean
    };

export type AvailableMediaTypeCompositionResponseModel = {
        id: string
name: string
icon: string
folderPath: Array<string>
isCompatible: boolean
    };

export type AvailableMemberTypeCompositionResponseModel = {
        id: string
name: string
icon: string
folderPath: Array<string>
isCompatible: boolean
    };

export type ChangePasswordCurrentUserRequestModel = {
        newPassword: string
oldPassword?: string | null
    };

export type ChangePasswordUserRequestModel = {
        newPassword: string
    };

export enum CompositionTypeModel {
    COMPOSITION = 'Composition',
    INHERITANCE = 'Inheritance'
}

export type ConsentLevelPresentationModel = {
        level: TelemetryLevelModel
description: string
    };

export type CopyDataTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type CopyDocumentRequestModel = {
        target?: ReferenceByIdModel | null
relateToOriginal: boolean
includeDescendants: boolean
    };

export type CopyDocumentTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type CopyMediaTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type CreateDataTypeRequestModel = {
        name: string
editorAlias: string
editorUiAlias: string
values: Array<DataTypePropertyPresentationModel>
id?: string | null
parent?: ReferenceByIdModel | null
    };

export type CreateDictionaryItemRequestModel = {
        name: string
translations: Array<DictionaryItemTranslationModel>
id?: string | null
parent?: ReferenceByIdModel | null
    };

export type CreateDocumentBlueprintFromDocumentRequestModel = {
        document: ReferenceByIdModel
id?: string | null
name: string
parent?: ReferenceByIdModel | null
    };

export type CreateDocumentBlueprintRequestModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantRequestModel>
id?: string | null
parent?: ReferenceByIdModel | null
documentType: ReferenceByIdModel
    };

export type CreateDocumentRequestModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantRequestModel>
id?: string | null
parent?: ReferenceByIdModel | null
documentType: ReferenceByIdModel
template: ReferenceByIdModel | null
    };

export type CreateDocumentTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type CreateDocumentTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type CreateDocumentTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<CreateDocumentTypePropertyTypeRequestModel>
containers: Array<CreateDocumentTypePropertyTypeContainerRequestModel>
id?: string | null
parent?: ReferenceByIdModel | null
allowedTemplates: Array<ReferenceByIdModel>
defaultTemplate?: ReferenceByIdModel | null
cleanup: DocumentTypeCleanupModel
allowedDocumentTypes: Array<DocumentTypeSortModel>
compositions: Array<DocumentTypeCompositionModel>
    };

export type CreateFolderRequestModel = {
        name: string
id?: string | null
parent?: ReferenceByIdModel | null
    };

export type CreateInitialPasswordUserRequestModel = {
        user: ReferenceByIdModel
token: string
password: string
    };

export type CreateLanguageRequestModel = {
        name: string
isDefault: boolean
isMandatory: boolean
fallbackIsoCode?: string | null
isoCode: string
    };

export type CreateMediaRequestModel = {
        values: Array<MediaValueModel>
variants: Array<MediaVariantRequestModel>
id?: string | null
parent?: ReferenceByIdModel | null
mediaType: ReferenceByIdModel
    };

export type CreateMediaTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type CreateMediaTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type CreateMediaTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
isElement: boolean
properties: Array<CreateMediaTypePropertyTypeRequestModel>
containers: Array<CreateMediaTypePropertyTypeContainerRequestModel>
id?: string | null
parent?: ReferenceByIdModel | null
allowedMediaTypes: Array<MediaTypeSortModel>
compositions: Array<MediaTypeCompositionModel>
collection?: ReferenceByIdModel | null
    };

export type CreateMemberGroupRequestModel = {
        name: string
id?: string | null
    };

export type CreateMemberRequestModel = {
        values: Array<MemberValueModel>
variants: Array<MemberVariantRequestModel>
id?: string | null
email: string
username: string
password: string
memberType: ReferenceByIdModel
groups?: Array<string> | null
isApproved: boolean
    };

export type CreateMemberTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type CreateMemberTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
isSensitive: boolean
visibility: MemberTypePropertyTypeVisibilityModel
    };

export type CreateMemberTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<CreateMemberTypePropertyTypeRequestModel>
containers: Array<CreateMemberTypePropertyTypeContainerRequestModel>
id?: string | null
compositions: Array<MemberTypeCompositionModel>
    };

export type CreatePackageRequestModel = {
        name: string
contentNodeId?: string | null
contentLoadChildNodes: boolean
mediaIds: Array<string>
mediaLoadChildNodes: boolean
documentTypes: Array<string>
mediaTypes: Array<string>
dataTypes: Array<string>
templates: Array<string>
partialViews: Array<string>
stylesheets: Array<string>
scripts: Array<string>
languages: Array<string>
dictionaryItems: Array<string>
id?: string | null
    };

export type CreatePartialViewFolderRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
    };

export type CreatePartialViewRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type CreateScriptFolderRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
    };

export type CreateScriptRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type CreateStylesheetFolderRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
    };

export type CreateStylesheetRequestModel = {
        name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type CreateTemplateRequestModel = {
        name: string
alias: string
content?: string | null
id?: string | null
    };

export type CreateUserDataRequestModel = {
        group: string
identifier: string
value: string
key?: string | null
    };

export type CreateUserGroupRequestModel = {
        name: string
alias: string
icon?: string | null
sections: Array<string>
languages: Array<string>
hasAccessToAllLanguages: boolean
documentStartNode?: ReferenceByIdModel | null
documentRootAccess: boolean
mediaStartNode?: ReferenceByIdModel | null
mediaRootAccess: boolean
fallbackPermissions: Array<string>
permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>
id?: string | null
    };

export type CreateUserRequestModel = {
        email: string
userName: string
name: string
userGroupIds: Array<ReferenceByIdModel>
id?: string | null
    };

export type CreateWebhookRequestModel = {
        enabled: boolean
url: string
contentTypeKeys: Array<string>
headers: Record<string, string>
id?: string | null
events: Array<string>
    };

export type CultureAndScheduleRequestModel = {
        culture?: string | null
schedule?: ScheduleRequestModel | null
    };

export type CultureReponseModel = {
        name: string
englishName: string
    };

export type CurrenUserConfigurationResponseModel = {
        keepUserLoggedIn: boolean
usernameIsEmail: boolean
passwordConfiguration: PasswordConfigurationResponseModel
    };

export type CurrentUserResponseModel = {
        id: string
email: string
userName: string
name: string
languageIsoCode?: string | null
documentStartNodeIds: Array<ReferenceByIdModel>
hasDocumentRootAccess: boolean
mediaStartNodeIds: Array<ReferenceByIdModel>
hasMediaRootAccess: boolean
avatarUrls: Array<string>
languages: Array<string>
hasAccessToAllLanguages: boolean
hasAccessToSensitiveData: boolean
fallbackPermissions: Array<string>
permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>
allowedSections: Array<string>
isAdmin: boolean
    };

export enum DataTypeChangeModeModel {
    TRUE = 'True',
    FALSE = 'False',
    FALSE_WITH_HELP_TEXT = 'FalseWithHelpText'
}

export type DataTypeContentTypeReferenceModel = {
        id: string
type: string | null
name: string | null
icon: string | null
    };

export type DataTypeItemResponseModel = {
        id: string
name: string
editorUiAlias?: string | null
isDeletable: boolean
    };

export type DataTypePropertyPresentationModel = {
        alias: string
value?: unknown
    };

export type DataTypePropertyReferenceModel = {
        name: string
alias: string
    };

export type DataTypeReferenceResponseModel = {
        contentType: DataTypeContentTypeReferenceModel
properties: Array<DataTypePropertyReferenceModel>
    };

export type DataTypeResponseModel = {
        name: string
editorAlias: string
editorUiAlias: string
values: Array<DataTypePropertyPresentationModel>
id: string
isDeletable: boolean
canIgnoreStartNodes: boolean
    };

export type DataTypeTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
isFolder: boolean
editorUiAlias?: string | null
isDeletable: boolean
    };

export type DatabaseInstallRequestModel = {
        id: string
providerName: string
server?: string | null
name?: string | null
username?: string | null
password?: string | null
useIntegratedAuthentication: boolean
connectionString?: string | null
trustServerCertificate: boolean
    };

export type DatabaseSettingsPresentationModel = {
        id: string
sortOrder: number
displayName: string
defaultDatabaseName: string
providerName: string
isConfigured: boolean
requiresServer: boolean
serverPlaceholder: string
requiresCredentials: boolean
supportsIntegratedAuthentication: boolean
requiresConnectionTest: boolean
    };

export type DatatypeConfigurationResponseModel = {
        canBeChanged: DataTypeChangeModeModel
documentListViewId: string
mediaListViewId: string
    };

export type DefaultReferenceResponseModel = {
        id: string
name?: string | null
type?: string | null
icon?: string | null
    };

export type DeleteUserGroupsRequestModel = {
        userGroupIds: Array<ReferenceByIdModel>
    };

export type DeleteUsersRequestModel = {
        userIds: Array<ReferenceByIdModel>
    };

export type DictionaryItemItemResponseModel = {
        id: string
name: string
    };

export type DictionaryItemResponseModel = {
        name: string
translations: Array<DictionaryItemTranslationModel>
id: string
    };

export type DictionaryItemTranslationModel = {
        isoCode: string
translation: string
    };

export type DictionaryOverviewResponseModel = {
        name?: string | null
id: string
parent?: ReferenceByIdModel | null
translatedIsoCodes: Array<string>
    };

export enum DirectionModel {
    ASCENDING = 'Ascending',
    DESCENDING = 'Descending'
}

export type DisableUserRequestModel = {
        userIds: Array<ReferenceByIdModel>
    };

export type DocumentBlueprintItemResponseModel = {
        id: string
name: string
documentType: DocumentTypeReferenceResponseModel
    };

export type DocumentBlueprintResponseModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantResponseModel>
id: string
documentType: DocumentTypeReferenceResponseModel
    };

export type DocumentBlueprintTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
isFolder: boolean
documentType?: DocumentTypeReferenceResponseModel | null
    };

export type DocumentCollectionResponseModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantResponseModel>
id: string
creator?: string | null
sortOrder: number
documentType: DocumentTypeCollectionReferenceResponseModel
updater?: string | null
    };

export type DocumentConfigurationResponseModel = {
        disableDeleteWhenReferenced: boolean
disableUnpublishWhenReferenced: boolean
allowEditInvariantFromNonDefault: boolean
allowNonExistingSegmentsCreation: boolean
reservedFieldNames: Array<string>
    };

export type DocumentItemResponseModel = {
        id: string
isTrashed: boolean
isProtected: boolean
documentType: DocumentTypeReferenceResponseModel
variants: Array<DocumentVariantItemResponseModel>
    };

export type DocumentNotificationResponseModel = {
        actionId: string
subscribed: boolean
    };

export type DocumentPermissionPresentationModel = {
        $type: string
document: ReferenceByIdModel
verbs: Array<string>
    };

export type DocumentRecycleBinItemResponseModel = {
        id: string
hasChildren: boolean
parent?: ItemReferenceByIdResponseModel | null
documentType: DocumentTypeReferenceResponseModel
variants: Array<DocumentVariantItemResponseModel>
    };

export type DocumentReferenceResponseModel = {
        id: string
name?: string | null
published?: boolean | null
documentType: TrackedReferenceDocumentTypeModel
    };

export type DocumentResponseModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantResponseModel>
id: string
documentType: DocumentTypeReferenceResponseModel
urls: Array<DocumentUrlInfoModel>
template?: ReferenceByIdModel | null
isTrashed: boolean
    };

export type DocumentTreeItemResponseModel = {
        hasChildren: boolean
parent?: ReferenceByIdModel | null
noAccess: boolean
isTrashed: boolean
id: string
isProtected: boolean
documentType: DocumentTypeReferenceResponseModel
variants: Array<DocumentVariantItemResponseModel>
    };

export type DocumentTypeBlueprintItemResponseModel = {
        id: string
name: string
    };

export type DocumentTypeCleanupModel = {
        preventCleanup: boolean
keepAllVersionsNewerThanDays?: number | null
keepLatestVersionPerDayForDays?: number | null
    };

export type DocumentTypeCollectionReferenceResponseModel = {
        id: string
alias: string
icon: string
    };

export type DocumentTypeCompositionModel = {
        documentType: ReferenceByIdModel
compositionType: CompositionTypeModel
    };

export type DocumentTypeCompositionRequestModel = {
        id?: string | null
currentPropertyAliases: Array<string>
currentCompositeIds: Array<string>
isElement: boolean
    };

export type DocumentTypeCompositionResponseModel = {
        id: string
name: string
icon: string
    };

export type DocumentTypeConfigurationResponseModel = {
        dataTypesCanBeChanged: DataTypeChangeModeModel
disableTemplates: boolean
useSegments: boolean
    };

export type DocumentTypeItemResponseModel = {
        id: string
name: string
isElement: boolean
icon?: string | null
description?: string | null
    };

export type DocumentTypePropertyTypeContainerResponseModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type DocumentTypePropertyTypeResponseModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type DocumentTypeReferenceResponseModel = {
        id: string
icon: string
collection?: ReferenceByIdModel | null
    };

export type DocumentTypeResponseModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<DocumentTypePropertyTypeResponseModel>
containers: Array<DocumentTypePropertyTypeContainerResponseModel>
id: string
allowedTemplates: Array<ReferenceByIdModel>
defaultTemplate?: ReferenceByIdModel | null
cleanup: DocumentTypeCleanupModel
allowedDocumentTypes: Array<DocumentTypeSortModel>
compositions: Array<DocumentTypeCompositionModel>
    };

export type DocumentTypeSortModel = {
        documentType: ReferenceByIdModel
sortOrder: number
    };

export type DocumentTypeTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
isFolder: boolean
isElement: boolean
icon: string
    };

export type DocumentUrlInfoModel = {
        culture: string | null
url: string
    };

export type DocumentUrlInfoResponseModel = {
        id: string
urlInfos: Array<DocumentUrlInfoModel>
    };

export type DocumentValueModel = {
        culture?: string | null
segment?: string | null
alias: string
value?: unknown
    };

export type DocumentVariantItemResponseModel = {
        name: string
culture?: string | null
state: DocumentVariantStateModel
    };

export type DocumentVariantRequestModel = {
        culture?: string | null
segment?: string | null
name: string
    };

export type DocumentVariantResponseModel = {
        culture?: string | null
segment?: string | null
name: string
createDate: string
updateDate: string
state: DocumentVariantStateModel
publishDate?: string | null
    };

export enum DocumentVariantStateModel {
    NOT_CREATED = 'NotCreated',
    DRAFT = 'Draft',
    PUBLISHED = 'Published',
    PUBLISHED_PENDING_CHANGES = 'PublishedPendingChanges'
}

export type DocumentVersionItemResponseModel = {
        id: string
document: ReferenceByIdModel
documentType: ReferenceByIdModel
user: ReferenceByIdModel
versionDate: string
isCurrentPublishedVersion: boolean
isCurrentDraftVersion: boolean
preventCleanup: boolean
    };

export type DocumentVersionResponseModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantResponseModel>
id: string
documentType: DocumentTypeReferenceResponseModel
document?: ReferenceByIdModel | null
    };

export type DomainPresentationModel = {
        domainName: string
isoCode: string
    };

export type DomainsResponseModel = {
        defaultIsoCode?: string | null
domains: Array<DomainPresentationModel>
    };

export type DynamicRootContextRequestModel = {
        id?: string | null
parent: ReferenceByIdModel
culture?: string | null
segment?: string | null
    };

export type DynamicRootQueryOriginRequestModel = {
        alias: string
id?: string | null
    };

export type DynamicRootQueryRequestModel = {
        origin: DynamicRootQueryOriginRequestModel
steps: Array<DynamicRootQueryStepRequestModel>
    };

export type DynamicRootQueryStepRequestModel = {
        alias: string
documentTypeIds: Array<string>
    };

export type DynamicRootRequestModel = {
        context: DynamicRootContextRequestModel
query: DynamicRootQueryRequestModel
    };

export type DynamicRootResponseModel = {
        roots: Array<string>
    };

export type EnableTwoFactorRequestModel = {
        code: string
secret: string
    };

export type EnableUserRequestModel = {
        userIds: Array<ReferenceByIdModel>
    };

export type EntityImportAnalysisResponseModel = {
        entityType: string
alias?: string | null
key?: string | null
    };

export enum EventMessageTypeModel {
    DEFAULT = 'Default',
    INFO = 'Info',
    ERROR = 'Error',
    SUCCESS = 'Success',
    WARNING = 'Warning'
}

export type FieldPresentationModel = {
        name: string
values: Array<string>
    };

export type FileSystemFolderModel = {
        path: string
    };

export type FileSystemTreeItemPresentationModel = {
        hasChildren: boolean
name: string
path: string
parent?: FileSystemFolderModel | null
isFolder: boolean
    };

export type FolderResponseModel = {
        name: string
id: string
    };

export type HealthCheckActionRequestModel = {
        healthCheck: ReferenceByIdModel
alias?: string | null
name?: string | null
description?: string | null
valueRequired: boolean
providedValue?: string | null
providedValueValidation?: string | null
providedValueValidationRegex?: string | null
    };

export type HealthCheckGroupPresentationModel = {
        name: string
checks: Array<HealthCheckModel>
    };

export type HealthCheckGroupResponseModel = {
        name: string
    };

export type HealthCheckGroupWithResultResponseModel = {
        checks: Array<HealthCheckWithResultPresentationModel>
    };

export type HealthCheckModel = {
        id: string
name: string
description?: string | null
    };

export type HealthCheckResultResponseModel = {
        message: string
resultType: StatusResultTypeModel
actions?: Array<HealthCheckActionRequestModel> | null
readMoreLink?: string | null
    };

export type HealthCheckWithResultPresentationModel = {
        id: string
results?: Array<HealthCheckResultResponseModel> | null
    };

export enum HealthStatusModel {
    HEALTHY = 'Healthy',
    UNHEALTHY = 'Unhealthy',
    REBUILDING = 'Rebuilding'
}

export type HealthStatusResponseModel = {
        status: HealthStatusModel
message?: string | null
    };

export type HelpPageResponseModel = {
        name?: string | null
description?: string | null
url?: string | null
type?: string | null
    };

export enum ImageCropModeModel {
    CROP = 'Crop',
    MAX = 'Max',
    STRETCH = 'Stretch',
    PAD = 'Pad',
    BOX_PAD = 'BoxPad',
    MIN = 'Min'
}

export type ImportDictionaryRequestModel = {
        temporaryFile: ReferenceByIdModel
parent?: ReferenceByIdModel | null
    };

export type ImportDocumentTypeRequestModel = {
        file: ReferenceByIdModel
    };

export type ImportMediaTypeRequestModel = {
        file: ReferenceByIdModel
    };

export type IndexResponseModel = {
        name: string
healthStatus: HealthStatusResponseModel
canRebuild: boolean
searcherName: string
documentCount: number
fieldCount: number
providerProperties?: Record<string, unknown> | null
    };

export type InstallRequestModel = {
        user: UserInstallRequestModel
database: DatabaseInstallRequestModel
telemetryLevel: TelemetryLevelModel
    };

export type InstallSettingsResponseModel = {
        user: UserSettingsPresentationModel
databases: Array<DatabaseSettingsPresentationModel>
    };

export type InviteUserRequestModel = {
        email: string
userName: string
name: string
userGroupIds: Array<ReferenceByIdModel>
id?: string | null
message?: string | null
    };

export type ItemReferenceByIdResponseModel = {
        id: string
    };

export type ItemSortingRequestModel = {
        id: string
sortOrder: number
    };

export type LanguageItemResponseModel = {
        name: string
isoCode: string
    };

export type LanguageResponseModel = {
        name: string
isDefault: boolean
isMandatory: boolean
fallbackIsoCode?: string | null
isoCode: string
    };

export type LogLevelCountsReponseModel = {
        information: number
debug: number
warning: number
error: number
fatal: number
    };

export enum LogLevelModel {
    VERBOSE = 'Verbose',
    DEBUG = 'Debug',
    INFORMATION = 'Information',
    WARNING = 'Warning',
    ERROR = 'Error',
    FATAL = 'Fatal'
}

export type LogMessagePropertyPresentationModel = {
        name: string
value?: string | null
    };

export type LogMessageResponseModel = {
        timestamp: string
level: LogLevelModel
messageTemplate?: string | null
renderedMessage?: string | null
properties: Array<LogMessagePropertyPresentationModel>
exception?: string | null
    };

export type LogTemplateResponseModel = {
        messageTemplate?: string | null
count: number
    };

export type LoggerResponseModel = {
        name: string
level: LogLevelModel
    };

export type ManifestResponseModel = {
        name: string
version?: string | null
extensions: Array<unknown>
    };

export type MediaCollectionResponseModel = {
        values: Array<MediaValueModel>
variants: Array<MediaVariantResponseModel>
id: string
creator?: string | null
sortOrder: number
mediaType: MediaTypeCollectionReferenceResponseModel
    };

export type MediaConfigurationResponseModel = {
        disableDeleteWhenReferenced: boolean
disableUnpublishWhenReferenced: boolean
reservedFieldNames: Array<string>
    };

export type MediaItemResponseModel = {
        id: string
isTrashed: boolean
mediaType: MediaTypeReferenceResponseModel
variants: Array<VariantItemResponseModel>
    };

export type MediaRecycleBinItemResponseModel = {
        id: string
hasChildren: boolean
parent?: ItemReferenceByIdResponseModel | null
mediaType: MediaTypeReferenceResponseModel
variants: Array<VariantItemResponseModel>
    };

export type MediaReferenceResponseModel = {
        id: string
name?: string | null
mediaType: TrackedReferenceMediaTypeModel
    };

export type MediaResponseModel = {
        values: Array<MediaValueModel>
variants: Array<MediaVariantResponseModel>
id: string
urls: Array<MediaUrlInfoModel>
isTrashed: boolean
mediaType: MediaTypeReferenceResponseModel
    };

export type MediaTreeItemResponseModel = {
        hasChildren: boolean
parent?: ReferenceByIdModel | null
noAccess: boolean
isTrashed: boolean
id: string
mediaType: MediaTypeReferenceResponseModel
variants: Array<VariantItemResponseModel>
    };

export type MediaTypeCollectionReferenceResponseModel = {
        id: string
alias: string
icon: string
    };

export type MediaTypeCompositionModel = {
        mediaType: ReferenceByIdModel
compositionType: CompositionTypeModel
    };

export type MediaTypeCompositionRequestModel = {
        id?: string | null
currentPropertyAliases: Array<string>
currentCompositeIds: Array<string>
    };

export type MediaTypeCompositionResponseModel = {
        id: string
name: string
icon: string
    };

export type MediaTypeItemResponseModel = {
        id: string
name: string
icon?: string | null
    };

export type MediaTypePropertyTypeContainerResponseModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type MediaTypePropertyTypeResponseModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type MediaTypeReferenceResponseModel = {
        id: string
icon: string
collection?: ReferenceByIdModel | null
    };

export type MediaTypeResponseModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<MediaTypePropertyTypeResponseModel>
containers: Array<MediaTypePropertyTypeContainerResponseModel>
id: string
allowedMediaTypes: Array<MediaTypeSortModel>
compositions: Array<MediaTypeCompositionModel>
isDeletable: boolean
aliasCanBeChanged: boolean
    };

export type MediaTypeSortModel = {
        mediaType: ReferenceByIdModel
sortOrder: number
    };

export type MediaTypeTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
isFolder: boolean
icon: string
isDeletable: boolean
    };

export type MediaUrlInfoModel = {
        culture: string | null
url: string
    };

export type MediaUrlInfoResponseModel = {
        id: string
urlInfos: Array<MediaUrlInfoModel>
    };

export type MediaValueModel = {
        culture?: string | null
segment?: string | null
alias: string
value?: unknown
    };

export type MediaVariantRequestModel = {
        culture?: string | null
segment?: string | null
name: string
    };

export type MediaVariantResponseModel = {
        culture?: string | null
segment?: string | null
name: string
createDate: string
updateDate: string
    };

export type MemberConfigurationResponseModel = {
        reservedFieldNames: Array<string>
    };

export type MemberGroupItemResponseModel = {
        id: string
name: string
    };

export type MemberGroupResponseModel = {
        name: string
id: string
    };

export type MemberItemResponseModel = {
        id: string
memberType: MemberTypeReferenceResponseModel
variants: Array<VariantItemResponseModel>
    };

export type MemberResponseModel = {
        values: Array<MemberValueModel>
variants: Array<MemberVariantResponseModel>
id: string
email: string
username: string
memberType: MemberTypeReferenceResponseModel
isApproved: boolean
isLockedOut: boolean
isTwoFactorEnabled: boolean
failedPasswordAttempts: number
lastLoginDate?: string | null
lastLockoutDate?: string | null
lastPasswordChangeDate?: string | null
groups: Array<string>
    };

export type MemberTypeCompositionModel = {
        memberType: ReferenceByIdModel
compositionType: CompositionTypeModel
    };

export type MemberTypeCompositionRequestModel = {
        id?: string | null
currentPropertyAliases: Array<string>
currentCompositeIds: Array<string>
    };

export type MemberTypeCompositionResponseModel = {
        id: string
name: string
icon: string
    };

export type MemberTypeItemResponseModel = {
        id: string
name: string
icon?: string | null
    };

export type MemberTypePropertyTypeContainerResponseModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type MemberTypePropertyTypeResponseModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
isSensitive: boolean
visibility: MemberTypePropertyTypeVisibilityModel
    };

export type MemberTypePropertyTypeVisibilityModel = {
        memberCanView: boolean
memberCanEdit: boolean
    };

export type MemberTypeReferenceResponseModel = {
        id: string
icon: string
collection?: ReferenceByIdModel | null
    };

export type MemberTypeResponseModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<MemberTypePropertyTypeResponseModel>
containers: Array<MemberTypePropertyTypeContainerResponseModel>
id: string
compositions: Array<MemberTypeCompositionModel>
    };

export type MemberTypeTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
icon: string
    };

export type MemberValueModel = {
        culture?: string | null
segment?: string | null
alias: string
value?: unknown
    };

export type MemberVariantRequestModel = {
        culture?: string | null
segment?: string | null
name: string
    };

export type MemberVariantResponseModel = {
        culture?: string | null
segment?: string | null
name: string
createDate: string
updateDate: string
    };

export type ModelsBuilderResponseModel = {
        mode: ModelsModeModel
canGenerate: boolean
outOfDateModels: boolean
lastError?: string | null
version?: string | null
modelsNamespace?: string | null
trackingOutOfDateModels: boolean
    };

export enum ModelsModeModel {
    NOTHING = 'Nothing',
    IN_MEMORY_AUTO = 'InMemoryAuto',
    SOURCE_CODE_MANUAL = 'SourceCodeManual',
    SOURCE_CODE_AUTO = 'SourceCodeAuto'
}

export type MoveDataTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveDictionaryRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveDocumentBlueprintRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveDocumentRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveDocumentTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveMediaRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type MoveMediaTypeRequestModel = {
        target?: ReferenceByIdModel | null
    };

export type NamedEntityTreeItemResponseModel = {
        hasChildren: boolean
id: string
parent?: ReferenceByIdModel | null
name: string
    };

export type NoopSetupTwoFactorModel = Record<string, unknown>;

export type NotificationHeaderModel = {
        message: string
category: string
type: EventMessageTypeModel
    };

export type OEmbedResponseModel = {
        markup: string
    };

export type ObjectTypeResponseModel = {
        name?: string | null
id: string
    };

export enum OperatorModel {
    EQUALS = 'Equals',
    NOT_EQUALS = 'NotEquals',
    CONTAINS = 'Contains',
    NOT_CONTAINS = 'NotContains',
    LESS_THAN = 'LessThan',
    LESS_THAN_EQUAL_TO = 'LessThanEqualTo',
    GREATER_THAN = 'GreaterThan',
    GREATER_THAN_EQUAL_TO = 'GreaterThanEqualTo'
}

export type OutOfDateStatusResponseModel = {
        status: OutOfDateTypeModel
    };

export enum OutOfDateTypeModel {
    OUT_OF_DATE = 'OutOfDate',
    CURRENT = 'Current',
    UNKNOWN = 'Unknown'
}

export type PackageConfigurationResponseModel = {
        marketplaceUrl: string
    };

export type PackageDefinitionResponseModel = {
        name: string
contentNodeId?: string | null
contentLoadChildNodes: boolean
mediaIds: Array<string>
mediaLoadChildNodes: boolean
documentTypes: Array<string>
mediaTypes: Array<string>
dataTypes: Array<string>
templates: Array<string>
partialViews: Array<string>
stylesheets: Array<string>
scripts: Array<string>
languages: Array<string>
dictionaryItems: Array<string>
id: string
packagePath: string
    };

export type PackageMigrationStatusResponseModel = {
        packageName: string
hasPendingMigrations: boolean
    };

export type PagedAllowedDocumentTypeModel = {
        total: number
items: Array<AllowedDocumentTypeModel>
    };

export type PagedAllowedMediaTypeModel = {
        total: number
items: Array<AllowedMediaTypeModel>
    };

export type PagedAuditLogResponseModel = {
        total: number
items: Array<AuditLogResponseModel>
    };

export type PagedCultureReponseModel = {
        total: number
items: Array<CultureReponseModel>
    };

export type PagedDataTypeItemResponseModel = {
        total: number
items: Array<DataTypeItemResponseModel>
    };

export type PagedDataTypeTreeItemResponseModel = {
        total: number
items: Array<DataTypeTreeItemResponseModel>
    };

export type PagedDictionaryOverviewResponseModel = {
        total: number
items: Array<DictionaryOverviewResponseModel>
    };

export type PagedDocumentBlueprintTreeItemResponseModel = {
        total: number
items: Array<DocumentBlueprintTreeItemResponseModel>
    };

export type PagedDocumentCollectionResponseModel = {
        total: number
items: Array<DocumentCollectionResponseModel>
    };

export type PagedDocumentRecycleBinItemResponseModel = {
        total: number
items: Array<DocumentRecycleBinItemResponseModel>
    };

export type PagedDocumentTreeItemResponseModel = {
        total: number
items: Array<DocumentTreeItemResponseModel>
    };

export type PagedDocumentTypeBlueprintItemResponseModel = {
        total: number
items: Array<DocumentTypeBlueprintItemResponseModel>
    };

export type PagedDocumentTypeTreeItemResponseModel = {
        total: number
items: Array<DocumentTypeTreeItemResponseModel>
    };

export type PagedDocumentVersionItemResponseModel = {
        total: number
items: Array<DocumentVersionItemResponseModel>
    };

export type PagedFileSystemTreeItemPresentationModel = {
        total: number
items: Array<FileSystemTreeItemPresentationModel>
    };

export type PagedHealthCheckGroupResponseModel = {
        total: number
items: Array<HealthCheckGroupResponseModel>
    };

export type PagedHelpPageResponseModel = {
        total: number
items: Array<HelpPageResponseModel>
    };

export type PagedIReferenceResponseModel = {
        total: number
items: Array<DefaultReferenceResponseModel | DocumentReferenceResponseModel | MediaReferenceResponseModel>
    };

export type PagedIndexResponseModel = {
        total: number
items: Array<IndexResponseModel>
    };

export type PagedLanguageResponseModel = {
        total: number
items: Array<LanguageResponseModel>
    };

export type PagedLogMessageResponseModel = {
        total: number
items: Array<LogMessageResponseModel>
    };

export type PagedLogTemplateResponseModel = {
        total: number
items: Array<LogTemplateResponseModel>
    };

export type PagedLoggerResponseModel = {
        total: number
items: Array<LoggerResponseModel>
    };

export type PagedMediaCollectionResponseModel = {
        total: number
items: Array<MediaCollectionResponseModel>
    };

export type PagedMediaRecycleBinItemResponseModel = {
        total: number
items: Array<MediaRecycleBinItemResponseModel>
    };

export type PagedMediaTreeItemResponseModel = {
        total: number
items: Array<MediaTreeItemResponseModel>
    };

export type PagedMediaTypeTreeItemResponseModel = {
        total: number
items: Array<MediaTypeTreeItemResponseModel>
    };

export type PagedMemberGroupResponseModel = {
        total: number
items: Array<MemberGroupResponseModel>
    };

export type PagedMemberResponseModel = {
        total: number
items: Array<MemberResponseModel>
    };

export type PagedMemberTypeTreeItemResponseModel = {
        total: number
items: Array<MemberTypeTreeItemResponseModel>
    };

export type PagedModelDataTypeItemResponseModel = {
        items: Array<DataTypeItemResponseModel>
total: number
    };

export type PagedModelDocumentItemResponseModel = {
        items: Array<DocumentItemResponseModel>
total: number
    };

export type PagedModelDocumentTypeItemResponseModel = {
        items: Array<DocumentTypeItemResponseModel>
total: number
    };

export type PagedModelMediaItemResponseModel = {
        items: Array<MediaItemResponseModel>
total: number
    };

export type PagedModelMediaTypeItemResponseModel = {
        items: Array<MediaTypeItemResponseModel>
total: number
    };

export type PagedModelMemberItemResponseModel = {
        items: Array<MemberItemResponseModel>
total: number
    };

export type PagedModelMemberTypeItemResponseModel = {
        items: Array<MemberTypeItemResponseModel>
total: number
    };

export type PagedModelTemplateItemResponseModel = {
        items: Array<TemplateItemResponseModel>
total: number
    };

export type PagedNamedEntityTreeItemResponseModel = {
        total: number
items: Array<NamedEntityTreeItemResponseModel>
    };

export type PagedObjectTypeResponseModel = {
        total: number
items: Array<ObjectTypeResponseModel>
    };

export type PagedPackageDefinitionResponseModel = {
        total: number
items: Array<PackageDefinitionResponseModel>
    };

export type PagedPackageMigrationStatusResponseModel = {
        total: number
items: Array<PackageMigrationStatusResponseModel>
    };

export type PagedPartialViewSnippetItemResponseModel = {
        total: number
items: Array<PartialViewSnippetItemResponseModel>
    };

export type PagedProblemDetailsModel = {
        total: number
items: Array<ProblemDetails>
    };

export type PagedRedirectUrlResponseModel = {
        total: number
items: Array<RedirectUrlResponseModel>
    };

export type PagedReferenceByIdModel = {
        total: number
items: Array<ReferenceByIdModel>
    };

export type PagedRelationResponseModel = {
        total: number
items: Array<RelationResponseModel>
    };

export type PagedRelationTypeResponseModel = {
        total: number
items: Array<RelationTypeResponseModel>
    };

export type PagedSavedLogSearchResponseModel = {
        total: number
items: Array<SavedLogSearchResponseModel>
    };

export type PagedSearchResultResponseModel = {
        total: number
items: Array<SearchResultResponseModel>
    };

export type PagedSearcherResponseModel = {
        total: number
items: Array<SearcherResponseModel>
    };

export type PagedSegmentResponseModel = {
        total: number
items: Array<SegmentResponseModel>
    };

export type PagedTagResponseModel = {
        total: number
items: Array<TagResponseModel>
    };

export type PagedTelemetryResponseModel = {
        total: number
items: Array<TelemetryResponseModel>
    };

export type PagedUserDataResponseModel = {
        total: number
items: Array<UserDataResponseModel>
    };

export type PagedUserGroupResponseModel = {
        total: number
items: Array<UserGroupResponseModel>
    };

export type PagedUserResponseModel = {
        total: number
items: Array<UserResponseModel>
    };

export type PagedWebhookEventModel = {
        total: number
items: Array<WebhookEventModel>
    };

export type PagedWebhookResponseModel = {
        total: number
items: Array<WebhookResponseModel>
    };

export type PartialViewFolderResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
    };

export type PartialViewItemResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
isFolder: boolean
    };

export type PartialViewResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type PartialViewSnippetItemResponseModel = {
        id: string
name: string
    };

export type PartialViewSnippetResponseModel = {
        id: string
name: string
content: string
    };

export type PasswordConfigurationResponseModel = {
        minimumPasswordLength: number
requireNonLetterOrDigit: boolean
requireDigit: boolean
requireLowercase: boolean
requireUppercase: boolean
    };

export type ProblemDetails = {
        type?: string | null
title?: string | null
status?: number | null
detail?: string | null
instance?: string | null
[key: string]: unknown | undefined
    };

export type ProblemDetailsBuilderModel = Record<string, unknown>;

export type ProfilingStatusRequestModel = {
        enabled: boolean
    };

export type ProfilingStatusResponseModel = {
        enabled: boolean
    };

export type PropertyTypeAppearanceModel = {
        labelOnTop: boolean
    };

export type PropertyTypeValidationModel = {
        mandatory: boolean
mandatoryMessage?: string | null
regEx?: string | null
regExMessage?: string | null
    };

export type PublicAccessRequestModel = {
        loginDocument: ReferenceByIdModel
errorDocument: ReferenceByIdModel
memberUserNames: Array<string>
memberGroupNames: Array<string>
    };

export type PublicAccessResponseModel = {
        loginDocument: ReferenceByIdModel
errorDocument: ReferenceByIdModel
members: Array<MemberItemResponseModel>
groups: Array<MemberGroupItemResponseModel>
    };

export type PublishDocumentRequestModel = {
        publishSchedules: Array<CultureAndScheduleRequestModel>
    };

export type PublishDocumentWithDescendantsRequestModel = {
        includeUnpublishedDescendants: boolean
cultures: Array<string>
    };

export enum RedirectStatusModel {
    ENABLED = 'Enabled',
    DISABLED = 'Disabled'
}

export type RedirectUrlResponseModel = {
        id: string
originalUrl: string
destinationUrl: string
created: string
document: ReferenceByIdModel
culture?: string | null
    };

export type RedirectUrlStatusResponseModel = {
        status: RedirectStatusModel
userIsAdmin: boolean
    };

export type ReferenceByIdModel = {
        id: string
    };

export type RelationReferenceModel = {
        id: string
name?: string | null
    };

export type RelationResponseModel = {
        id: string
relationType: ReferenceByIdModel
parent: RelationReferenceModel
child: RelationReferenceModel
createDate: string
comment?: string | null
    };

export type RelationTypeItemResponseModel = {
        id: string
name: string
isDeletable: boolean
    };

export type RelationTypeResponseModel = {
        name: string
isBidirectional: boolean
isDependency: boolean
id: string
alias?: string | null
parentObject?: ObjectTypeResponseModel | null
childObject?: ObjectTypeResponseModel | null
    };

export type RenamePartialViewRequestModel = {
        name: string
    };

export type RenameScriptRequestModel = {
        name: string
    };

export type RenameStylesheetRequestModel = {
        name: string
    };

export type ResendInviteUserRequestModel = {
        user: ReferenceByIdModel
message?: string | null
    };

export type ResetPasswordRequestModel = {
        email: string
    };

export type ResetPasswordTokenRequestModel = {
        user: ReferenceByIdModel
resetCode: string
password: string
    };

export type ResetPasswordUserResponseModel = {
        resetPassword?: string | null
    };

export enum RuntimeLevelModel {
    UNKNOWN = 'Unknown',
    BOOT = 'Boot',
    INSTALL = 'Install',
    UPGRADE = 'Upgrade',
    RUN = 'Run',
    BOOT_FAILED = 'BootFailed'
}

export enum RuntimeModeModel {
    BACKOFFICE_DEVELOPMENT = 'BackofficeDevelopment',
    DEVELOPMENT = 'Development',
    PRODUCTION = 'Production'
}

export type SavedLogSearchRequestModel = {
        name: string
query: string
    };

export type SavedLogSearchResponseModel = {
        name: string
query: string
    };

export type ScheduleRequestModel = {
        publishTime?: string | null
unpublishTime?: string | null
    };

export type ScriptFolderResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
    };

export type ScriptItemResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
isFolder: boolean
    };

export type ScriptResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type SearchResultResponseModel = {
        id: string
score: number
readonly fieldCount: number
fields: Array<FieldPresentationModel>
    };

export type SearcherResponseModel = {
        name: string
    };

export type SecurityConfigurationResponseModel = {
        passwordConfiguration: PasswordConfigurationResponseModel
    };

export type SegmentResponseModel = {
        name: string
alias: string
    };

export type ServerConfigurationItemResponseModel = {
        name: string
data: string
    };

export type ServerConfigurationResponseModel = {
        allowPasswordReset: boolean
    };

export type ServerInformationResponseModel = {
        version: string
assemblyVersion: string
baseUtcOffset: string
runtimeMode: RuntimeModeModel
    };

export type ServerStatusResponseModel = {
        serverStatus: RuntimeLevelModel
    };

export type ServerTroubleshootingResponseModel = {
        items: Array<ServerConfigurationItemResponseModel>
    };

export type SetAvatarRequestModel = {
        file: ReferenceByIdModel
    };

export type SortingRequestModel = {
        parent?: ReferenceByIdModel | null
sorting: Array<ItemSortingRequestModel>
    };

export type StaticFileItemResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
isFolder: boolean
    };

export enum StatusResultTypeModel {
    SUCCESS = 'Success',
    WARNING = 'Warning',
    ERROR = 'Error',
    INFO = 'Info'
}

export type StylesheetFolderResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
    };

export type StylesheetItemResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
isFolder: boolean
    };

export type StylesheetResponseModel = {
        path: string
name: string
parent?: FileSystemFolderModel | null
content: string
    };

export type TagResponseModel = {
        id: string
text?: string | null
group?: string | null
nodeCount: number
    };

export enum TelemetryLevelModel {
    MINIMAL = 'Minimal',
    BASIC = 'Basic',
    DETAILED = 'Detailed'
}

export type TelemetryRequestModel = {
        telemetryLevel: TelemetryLevelModel
    };

export type TelemetryResponseModel = {
        telemetryLevel: TelemetryLevelModel
    };

export type TemplateConfigurationResponseModel = {
        disabled: boolean
    };

export type TemplateItemResponseModel = {
        id: string
name: string
alias: string
    };

export type TemplateQueryExecuteFilterPresentationModel = {
        propertyAlias: string
constraintValue: string
operator: OperatorModel
    };

export type TemplateQueryExecuteModel = {
        rootDocument?: ReferenceByIdModel | null
documentTypeAlias?: string | null
filters?: Array<TemplateQueryExecuteFilterPresentationModel> | null
sort?: TemplateQueryExecuteSortModel | null
take: number
    };

export type TemplateQueryExecuteSortModel = {
        propertyAlias: string
direction?: string | null
    };

export type TemplateQueryOperatorModel = {
        operator: OperatorModel
applicableTypes: Array<TemplateQueryPropertyTypeModel>
    };

export type TemplateQueryPropertyPresentationModel = {
        alias: string
type: TemplateQueryPropertyTypeModel
    };

export enum TemplateQueryPropertyTypeModel {
    STRING = 'String',
    DATE_TIME = 'DateTime',
    INTEGER = 'Integer'
}

export type TemplateQueryResultItemPresentationModel = {
        icon: string
name: string
    };

export type TemplateQueryResultResponseModel = {
        queryExpression: string
sampleResults: Array<TemplateQueryResultItemPresentationModel>
resultCount: number
executionTime: number
    };

export type TemplateQuerySettingsResponseModel = {
        documentTypeAliases: Array<string>
properties: Array<TemplateQueryPropertyPresentationModel>
operators: Array<TemplateQueryOperatorModel>
    };

export type TemplateResponseModel = {
        name: string
alias: string
content?: string | null
id: string
masterTemplate?: ReferenceByIdModel | null
    };

export type TemporaryFileConfigurationResponseModel = {
        imageFileTypes: Array<string>
disallowedUploadedFilesExtensions: Array<string>
allowedUploadedFileExtensions: Array<string>
maxFileSize?: number | null
    };

export type TemporaryFileResponseModel = {
        id: string
availableUntil?: string | null
fileName: string
    };

export type TrackedReferenceDocumentTypeModel = {
        icon?: string | null
alias?: string | null
name?: string | null
    };

export type TrackedReferenceMediaTypeModel = {
        icon?: string | null
alias?: string | null
name?: string | null
    };

export type UnknownTypePermissionPresentationModel = {
        $type: string
verbs: Array<string>
context: string
    };

export type UnlockUsersRequestModel = {
        userIds: Array<ReferenceByIdModel>
    };

export type UnpublishDocumentRequestModel = {
        cultures?: Array<string> | null
    };

export type UpdateDataTypeRequestModel = {
        name: string
editorAlias: string
editorUiAlias: string
values: Array<DataTypePropertyPresentationModel>
    };

export type UpdateDictionaryItemRequestModel = {
        name: string
translations: Array<DictionaryItemTranslationModel>
    };

export type UpdateDocumentBlueprintRequestModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantRequestModel>
    };

export type UpdateDocumentNotificationsRequestModel = {
        subscribedActionIds: Array<string>
    };

export type UpdateDocumentRequestModel = {
        values: Array<DocumentValueModel>
variants: Array<DocumentVariantRequestModel>
template?: ReferenceByIdModel | null
    };

export type UpdateDocumentTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type UpdateDocumentTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type UpdateDocumentTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<UpdateDocumentTypePropertyTypeRequestModel>
containers: Array<UpdateDocumentTypePropertyTypeContainerRequestModel>
allowedTemplates: Array<ReferenceByIdModel>
defaultTemplate?: ReferenceByIdModel | null
cleanup: DocumentTypeCleanupModel
allowedDocumentTypes: Array<DocumentTypeSortModel>
compositions: Array<DocumentTypeCompositionModel>
    };

export type UpdateDomainsRequestModel = {
        defaultIsoCode?: string | null
domains: Array<DomainPresentationModel>
    };

export type UpdateFolderResponseModel = {
        name: string
    };

export type UpdateLanguageRequestModel = {
        name: string
isDefault: boolean
isMandatory: boolean
fallbackIsoCode?: string | null
    };

export type UpdateMediaRequestModel = {
        values: Array<MediaValueModel>
variants: Array<MediaVariantRequestModel>
    };

export type UpdateMediaTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type UpdateMediaTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
    };

export type UpdateMediaTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<UpdateMediaTypePropertyTypeRequestModel>
containers: Array<UpdateMediaTypePropertyTypeContainerRequestModel>
allowedMediaTypes: Array<MediaTypeSortModel>
compositions: Array<MediaTypeCompositionModel>
    };

export type UpdateMemberGroupRequestModel = {
        name: string
    };

export type UpdateMemberRequestModel = {
        values: Array<MemberValueModel>
variants: Array<MemberVariantRequestModel>
email: string
username: string
oldPassword?: string | null
newPassword?: string | null
groups?: Array<string> | null
isApproved: boolean
isLockedOut: boolean
isTwoFactorEnabled: boolean
    };

export type UpdateMemberTypePropertyTypeContainerRequestModel = {
        id: string
parent?: ReferenceByIdModel | null
name?: string | null
type: string
sortOrder: number
    };

export type UpdateMemberTypePropertyTypeRequestModel = {
        id: string
container?: ReferenceByIdModel | null
sortOrder: number
alias: string
name: string
description?: string | null
dataType: ReferenceByIdModel
variesByCulture: boolean
variesBySegment: boolean
validation: PropertyTypeValidationModel
appearance: PropertyTypeAppearanceModel
isSensitive: boolean
visibility: MemberTypePropertyTypeVisibilityModel
    };

export type UpdateMemberTypeRequestModel = {
        alias: string
name: string
description?: string | null
icon: string
allowedAsRoot: boolean
variesByCulture: boolean
variesBySegment: boolean
collection?: ReferenceByIdModel | null
isElement: boolean
properties: Array<UpdateMemberTypePropertyTypeRequestModel>
containers: Array<UpdateMemberTypePropertyTypeContainerRequestModel>
compositions: Array<MemberTypeCompositionModel>
    };

export type UpdatePackageRequestModel = {
        name: string
contentNodeId?: string | null
contentLoadChildNodes: boolean
mediaIds: Array<string>
mediaLoadChildNodes: boolean
documentTypes: Array<string>
mediaTypes: Array<string>
dataTypes: Array<string>
templates: Array<string>
partialViews: Array<string>
stylesheets: Array<string>
scripts: Array<string>
languages: Array<string>
dictionaryItems: Array<string>
packagePath: string
    };

export type UpdatePartialViewRequestModel = {
        content: string
    };

export type UpdateScriptRequestModel = {
        content: string
    };

export type UpdateStylesheetRequestModel = {
        content: string
    };

export type UpdateTemplateRequestModel = {
        name: string
alias: string
content?: string | null
    };

export type UpdateUserDataRequestModel = {
        group: string
identifier: string
value: string
key: string
    };

export type UpdateUserGroupRequestModel = {
        name: string
alias: string
icon?: string | null
sections: Array<string>
languages: Array<string>
hasAccessToAllLanguages: boolean
documentStartNode?: ReferenceByIdModel | null
documentRootAccess: boolean
mediaStartNode?: ReferenceByIdModel | null
mediaRootAccess: boolean
fallbackPermissions: Array<string>
permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>
    };

export type UpdateUserGroupsOnUserRequestModel = {
        userIds: Array<ReferenceByIdModel>
userGroupIds: Array<ReferenceByIdModel>
    };

export type UpdateUserRequestModel = {
        email: string
userName: string
name: string
userGroupIds: Array<ReferenceByIdModel>
languageIsoCode: string
documentStartNodeIds: Array<ReferenceByIdModel>
hasDocumentRootAccess: boolean
mediaStartNodeIds: Array<ReferenceByIdModel>
hasMediaRootAccess: boolean
    };

export type UpdateWebhookRequestModel = {
        enabled: boolean
url: string
contentTypeKeys: Array<string>
headers: Record<string, string>
events: Array<string>
    };

export type UpgradeSettingsResponseModel = {
        currentState: string
newState: string
newVersion: string
oldVersion: string
readonly reportUrl: string
    };

export type UserConfigurationResponseModel = {
        canInviteUsers: boolean
passwordConfiguration: PasswordConfigurationResponseModel
    };

export type UserDataModel = {
        group: string
identifier: string
value: string
    };

export enum UserDataOperationStatusModel {
    SUCCESS = 'Success',
    NOT_FOUND = 'NotFound',
    USER_NOT_FOUND = 'UserNotFound',
    ALREADY_EXISTS = 'AlreadyExists'
}

export type UserDataResponseModel = {
        group: string
identifier: string
value: string
key: string
    };

export type UserExternalLoginProviderModel = {
        providerSchemeName: string
providerKey?: string | null
isLinkedOnUser: boolean
hasManualLinkingEnabled: boolean
    };

export type UserGroupItemResponseModel = {
        id: string
name: string
icon?: string | null
alias?: string | null
    };

export type UserGroupResponseModel = {
        name: string
alias: string
icon?: string | null
sections: Array<string>
languages: Array<string>
hasAccessToAllLanguages: boolean
documentStartNode?: ReferenceByIdModel | null
documentRootAccess: boolean
mediaStartNode?: ReferenceByIdModel | null
mediaRootAccess: boolean
fallbackPermissions: Array<string>
permissions: Array<DocumentPermissionPresentationModel | UnknownTypePermissionPresentationModel>
id: string
isDeletable: boolean
aliasCanBeChanged: boolean
    };

export type UserInstallRequestModel = {
        name: string
email: string
password: string
readonly subscribeToNewsletter: boolean
    };

export type UserItemResponseModel = {
        id: string
name: string
avatarUrls: Array<string>
    };

export enum UserOrderModel {
    USER_NAME = 'UserName',
    LANGUAGE = 'Language',
    NAME = 'Name',
    EMAIL = 'Email',
    ID = 'Id',
    CREATE_DATE = 'CreateDate',
    UPDATE_DATE = 'UpdateDate',
    IS_APPROVED = 'IsApproved',
    IS_LOCKED_OUT = 'IsLockedOut',
    LAST_LOGIN_DATE = 'LastLoginDate'
}

export type UserPermissionModel = {
        nodeKey: string
permissions: Array<string>
    };

export type UserPermissionsResponseModel = {
        permissions: Array<UserPermissionModel>
    };

export type UserResponseModel = {
        email: string
userName: string
name: string
userGroupIds: Array<ReferenceByIdModel>
id: string
languageIsoCode?: string | null
documentStartNodeIds: Array<ReferenceByIdModel>
hasDocumentRootAccess: boolean
mediaStartNodeIds: Array<ReferenceByIdModel>
hasMediaRootAccess: boolean
avatarUrls: Array<string>
state: UserStateModel
failedLoginAttempts: number
createDate: string
updateDate: string
lastLoginDate?: string | null
lastLockoutDate?: string | null
lastPasswordChangeDate?: string | null
isAdmin: boolean
    };

export type UserSettingsPresentationModel = {
        minCharLength: number
minNonAlphaNumericLength: number
consentLevels: Array<ConsentLevelPresentationModel>
    };

export enum UserStateModel {
    ACTIVE = 'Active',
    DISABLED = 'Disabled',
    LOCKED_OUT = 'LockedOut',
    INVITED = 'Invited',
    INACTIVE = 'Inactive',
    ALL = 'All'
}

export type UserTwoFactorProviderModel = {
        providerName: string
isEnabledOnUser: boolean
    };

export type VariantItemResponseModel = {
        name: string
culture?: string | null
    };

export type VerifyInviteUserRequestModel = {
        user: ReferenceByIdModel
token: string
    };

export type VerifyInviteUserResponseModel = {
        passwordConfiguration: PasswordConfigurationResponseModel
    };

export type VerifyResetPasswordResponseModel = {
        passwordConfiguration: PasswordConfigurationResponseModel
    };

export type VerifyResetPasswordTokenRequestModel = {
        user: ReferenceByIdModel
resetCode: string
    };

export type WebhookEventModel = {
        eventName: string
eventType: string
alias: string
    };

export type WebhookEventResponseModel = {
        eventName: string
eventType: string
alias: string
    };

export type WebhookItemResponseModel = {
        enabled: boolean
name: string
events: string
url: string
types: string
    };

export type WebhookResponseModel = {
        enabled: boolean
url: string
contentTypeKeys: Array<string>
headers: Record<string, string>
id: string
events: Array<WebhookEventResponseModel>
    };

export type CultureData = {

        payloads: {
            GetCulture: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetCulture: PagedCultureReponseModel

        }

    }

export type DataTypeData = {

        payloads: {
            PostDataType: {
                        requestBody?: CreateDataTypeRequestModel

                    };
GetDataTypeById: {
                        id: string

                    };
DeleteDataTypeById: {
                        id: string

                    };
PutDataTypeById: {
                        id: string
requestBody?: UpdateDataTypeRequestModel

                    };
PostDataTypeByIdCopy: {
                        id: string
requestBody?: CopyDataTypeRequestModel

                    };
GetDataTypeByIdIsUsed: {
                        id: string

                    };
PutDataTypeByIdMove: {
                        id: string
requestBody?: MoveDataTypeRequestModel

                    };
GetDataTypeByIdReferences: {
                        id: string

                    };
PostDataTypeFolder: {
                        requestBody?: CreateFolderRequestModel

                    };
GetDataTypeFolderById: {
                        id: string

                    };
DeleteDataTypeFolderById: {
                        id: string

                    };
PutDataTypeFolderById: {
                        id: string
requestBody?: UpdateFolderResponseModel

                    };
GetFilterDataType: {
                        editorAlias?: string
editorUiAlias?: string
name?: string
skip?: number
take?: number

                    };
GetItemDataType: {
                        id?: Array<string>

                    };
GetItemDataTypeSearch: {
                        query?: string
skip?: number
take?: number

                    };
GetTreeDataTypeAncestors: {
                        descendantId?: string

                    };
GetTreeDataTypeChildren: {
                        foldersOnly?: boolean
parentId?: string
skip?: number
take?: number

                    };
GetTreeDataTypeRoot: {
                        foldersOnly?: boolean
skip?: number
take?: number

                    };
        }


        responses: {
            PostDataType: string
                ,GetDataTypeById: DataTypeResponseModel
                ,DeleteDataTypeById: string
                ,PutDataTypeById: string
                ,PostDataTypeByIdCopy: string
                ,GetDataTypeByIdIsUsed: boolean
                ,PutDataTypeByIdMove: string
                ,GetDataTypeByIdReferences: Array<DataTypeReferenceResponseModel>
                ,GetDataTypeConfiguration: DatatypeConfigurationResponseModel
                ,PostDataTypeFolder: string
                ,GetDataTypeFolderById: FolderResponseModel
                ,DeleteDataTypeFolderById: string
                ,PutDataTypeFolderById: string
                ,GetFilterDataType: PagedDataTypeItemResponseModel
                ,GetItemDataType: Array<DataTypeItemResponseModel>
                ,GetItemDataTypeSearch: PagedModelDataTypeItemResponseModel
                ,GetTreeDataTypeAncestors: Array<DataTypeTreeItemResponseModel>
                ,GetTreeDataTypeChildren: PagedDataTypeTreeItemResponseModel
                ,GetTreeDataTypeRoot: PagedDataTypeTreeItemResponseModel

        }

    }

export type DictionaryData = {

        payloads: {
            GetDictionary: {
                        filter?: string
skip?: number
take?: number

                    };
PostDictionary: {
                        requestBody?: CreateDictionaryItemRequestModel

                    };
GetDictionaryById: {
                        id: string

                    };
DeleteDictionaryById: {
                        id: string

                    };
PutDictionaryById: {
                        id: string
requestBody?: UpdateDictionaryItemRequestModel

                    };
GetDictionaryByIdExport: {
                        id: string
includeChildren?: boolean

                    };
PutDictionaryByIdMove: {
                        id: string
requestBody?: MoveDictionaryRequestModel

                    };
PostDictionaryImport: {
                        requestBody?: ImportDictionaryRequestModel

                    };
GetItemDictionary: {
                        id?: Array<string>

                    };
GetTreeDictionaryAncestors: {
                        descendantId?: string

                    };
GetTreeDictionaryChildren: {
                        parentId?: string
skip?: number
take?: number

                    };
GetTreeDictionaryRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetDictionary: PagedDictionaryOverviewResponseModel
                ,PostDictionary: string
                ,GetDictionaryById: DictionaryItemResponseModel
                ,DeleteDictionaryById: string
                ,PutDictionaryById: string
                ,GetDictionaryByIdExport: Blob | File
                ,PutDictionaryByIdMove: string
                ,PostDictionaryImport: string
                ,GetItemDictionary: Array<DictionaryItemItemResponseModel>
                ,GetTreeDictionaryAncestors: Array<NamedEntityTreeItemResponseModel>
                ,GetTreeDictionaryChildren: PagedNamedEntityTreeItemResponseModel
                ,GetTreeDictionaryRoot: PagedNamedEntityTreeItemResponseModel

        }

    }

export type DocumentBlueprintData = {

        payloads: {
            PostDocumentBlueprint: {
                        requestBody?: CreateDocumentBlueprintRequestModel

                    };
GetDocumentBlueprintById: {
                        id: string

                    };
DeleteDocumentBlueprintById: {
                        id: string

                    };
PutDocumentBlueprintById: {
                        id: string
requestBody?: UpdateDocumentBlueprintRequestModel

                    };
PutDocumentBlueprintByIdMove: {
                        id: string
requestBody?: MoveDocumentBlueprintRequestModel

                    };
PostDocumentBlueprintFolder: {
                        requestBody?: CreateFolderRequestModel

                    };
GetDocumentBlueprintFolderById: {
                        id: string

                    };
DeleteDocumentBlueprintFolderById: {
                        id: string

                    };
PutDocumentBlueprintFolderById: {
                        id: string
requestBody?: UpdateFolderResponseModel

                    };
PostDocumentBlueprintFromDocument: {
                        requestBody?: CreateDocumentBlueprintFromDocumentRequestModel

                    };
GetItemDocumentBlueprint: {
                        id?: Array<string>

                    };
GetTreeDocumentBlueprintAncestors: {
                        descendantId?: string

                    };
GetTreeDocumentBlueprintChildren: {
                        foldersOnly?: boolean
parentId?: string
skip?: number
take?: number

                    };
GetTreeDocumentBlueprintRoot: {
                        foldersOnly?: boolean
skip?: number
take?: number

                    };
        }


        responses: {
            PostDocumentBlueprint: string
                ,GetDocumentBlueprintById: DocumentBlueprintResponseModel
                ,DeleteDocumentBlueprintById: string
                ,PutDocumentBlueprintById: string
                ,PutDocumentBlueprintByIdMove: string
                ,PostDocumentBlueprintFolder: string
                ,GetDocumentBlueprintFolderById: FolderResponseModel
                ,DeleteDocumentBlueprintFolderById: string
                ,PutDocumentBlueprintFolderById: string
                ,PostDocumentBlueprintFromDocument: string
                ,GetItemDocumentBlueprint: Array<DocumentBlueprintItemResponseModel>
                ,GetTreeDocumentBlueprintAncestors: Array<DocumentBlueprintTreeItemResponseModel>
                ,GetTreeDocumentBlueprintChildren: PagedDocumentBlueprintTreeItemResponseModel
                ,GetTreeDocumentBlueprintRoot: PagedDocumentBlueprintTreeItemResponseModel

        }

    }

export type DocumentTypeData = {

        payloads: {
            PostDocumentType: {
                        requestBody?: CreateDocumentTypeRequestModel

                    };
GetDocumentTypeById: {
                        id: string

                    };
DeleteDocumentTypeById: {
                        id: string

                    };
PutDocumentTypeById: {
                        id: string
requestBody?: UpdateDocumentTypeRequestModel

                    };
GetDocumentTypeByIdAllowedChildren: {
                        id: string
skip?: number
take?: number

                    };
GetDocumentTypeByIdBlueprint: {
                        id: string
skip?: number
take?: number

                    };
GetDocumentTypeByIdCompositionReferences: {
                        id: string

                    };
PostDocumentTypeByIdCopy: {
                        id: string
requestBody?: CopyDocumentTypeRequestModel

                    };
GetDocumentTypeByIdExport: {
                        id: string

                    };
PutDocumentTypeByIdImport: {
                        id: string
requestBody?: ImportDocumentTypeRequestModel

                    };
PutDocumentTypeByIdMove: {
                        id: string
requestBody?: MoveDocumentTypeRequestModel

                    };
GetDocumentTypeAllowedAtRoot: {
                        skip?: number
take?: number

                    };
PostDocumentTypeAvailableCompositions: {
                        requestBody?: DocumentTypeCompositionRequestModel

                    };
PostDocumentTypeFolder: {
                        requestBody?: CreateFolderRequestModel

                    };
GetDocumentTypeFolderById: {
                        id: string

                    };
DeleteDocumentTypeFolderById: {
                        id: string

                    };
PutDocumentTypeFolderById: {
                        id: string
requestBody?: UpdateFolderResponseModel

                    };
PostDocumentTypeImport: {
                        requestBody?: ImportDocumentTypeRequestModel

                    };
GetItemDocumentType: {
                        id?: Array<string>

                    };
GetItemDocumentTypeSearch: {
                        query?: string
skip?: number
take?: number

                    };
GetTreeDocumentTypeAncestors: {
                        descendantId?: string

                    };
GetTreeDocumentTypeChildren: {
                        foldersOnly?: boolean
parentId?: string
skip?: number
take?: number

                    };
GetTreeDocumentTypeRoot: {
                        foldersOnly?: boolean
skip?: number
take?: number

                    };
        }


        responses: {
            PostDocumentType: string
                ,GetDocumentTypeById: DocumentTypeResponseModel
                ,DeleteDocumentTypeById: string
                ,PutDocumentTypeById: string
                ,GetDocumentTypeByIdAllowedChildren: PagedAllowedDocumentTypeModel
                ,GetDocumentTypeByIdBlueprint: PagedDocumentTypeBlueprintItemResponseModel
                ,GetDocumentTypeByIdCompositionReferences: Array<DocumentTypeCompositionResponseModel>
                ,PostDocumentTypeByIdCopy: string
                ,GetDocumentTypeByIdExport: Blob | File
                ,PutDocumentTypeByIdImport: string
                ,PutDocumentTypeByIdMove: string
                ,GetDocumentTypeAllowedAtRoot: PagedAllowedDocumentTypeModel
                ,PostDocumentTypeAvailableCompositions: Array<AvailableDocumentTypeCompositionResponseModel>
                ,GetDocumentTypeConfiguration: DocumentTypeConfigurationResponseModel
                ,PostDocumentTypeFolder: string
                ,GetDocumentTypeFolderById: FolderResponseModel
                ,DeleteDocumentTypeFolderById: string
                ,PutDocumentTypeFolderById: string
                ,PostDocumentTypeImport: string
                ,GetItemDocumentType: Array<DocumentTypeItemResponseModel>
                ,GetItemDocumentTypeSearch: PagedModelDocumentTypeItemResponseModel
                ,GetTreeDocumentTypeAncestors: Array<DocumentTypeTreeItemResponseModel>
                ,GetTreeDocumentTypeChildren: PagedDocumentTypeTreeItemResponseModel
                ,GetTreeDocumentTypeRoot: PagedDocumentTypeTreeItemResponseModel

        }

    }

export type DocumentVersionData = {

        payloads: {
            GetDocumentVersion: {
                        culture?: string
documentId: string
skip?: number
take?: number

                    };
GetDocumentVersionById: {
                        id: string

                    };
PutDocumentVersionByIdPreventCleanup: {
                        id: string
preventCleanup?: boolean

                    };
PostDocumentVersionByIdRollback: {
                        culture?: string
id: string

                    };
        }


        responses: {
            GetDocumentVersion: PagedDocumentVersionItemResponseModel
                ,GetDocumentVersionById: DocumentVersionResponseModel
                ,PutDocumentVersionByIdPreventCleanup: string
                ,PostDocumentVersionByIdRollback: string

        }

    }

export type DocumentData = {

        payloads: {
            GetCollectionDocumentById: {
                        dataTypeId?: string
filter?: string
id: string
orderBy?: string
orderCulture?: string
orderDirection?: DirectionModel
skip?: number
take?: number

                    };
PostDocument: {
                        requestBody?: CreateDocumentRequestModel

                    };
GetDocumentById: {
                        id: string

                    };
DeleteDocumentById: {
                        id: string

                    };
PutDocumentById: {
                        id: string
requestBody?: UpdateDocumentRequestModel

                    };
GetDocumentByIdAuditLog: {
                        id: string
orderDirection?: DirectionModel
sinceDate?: string
skip?: number
take?: number

                    };
PostDocumentByIdCopy: {
                        id: string
requestBody?: CopyDocumentRequestModel

                    };
GetDocumentByIdDomains: {
                        id: string

                    };
PutDocumentByIdDomains: {
                        id: string
requestBody?: UpdateDomainsRequestModel

                    };
PutDocumentByIdMove: {
                        id: string
requestBody?: MoveDocumentRequestModel

                    };
PutDocumentByIdMoveToRecycleBin: {
                        id: string

                    };
GetDocumentByIdNotifications: {
                        id: string

                    };
PutDocumentByIdNotifications: {
                        id: string
requestBody?: UpdateDocumentNotificationsRequestModel

                    };
PostDocumentByIdPublicAccess: {
                        id: string
requestBody?: PublicAccessRequestModel

                    };
DeleteDocumentByIdPublicAccess: {
                        id: string

                    };
GetDocumentByIdPublicAccess: {
                        id: string

                    };
PutDocumentByIdPublicAccess: {
                        id: string
requestBody?: PublicAccessRequestModel

                    };
PutDocumentByIdPublish: {
                        id: string
requestBody?: PublishDocumentRequestModel

                    };
PutDocumentByIdPublishWithDescendants: {
                        id: string
requestBody?: PublishDocumentWithDescendantsRequestModel

                    };
GetDocumentByIdReferencedBy: {
                        id: string
skip?: number
take?: number

                    };
GetDocumentByIdReferencedDescendants: {
                        id: string
skip?: number
take?: number

                    };
PutDocumentByIdUnpublish: {
                        id: string
requestBody?: UnpublishDocumentRequestModel

                    };
PutDocumentByIdValidate: {
                        id: string
requestBody?: UpdateDocumentRequestModel

                    };
GetDocumentAreReferenced: {
                        id?: Array<string>
skip?: number
take?: number

                    };
PutDocumentSort: {
                        requestBody?: SortingRequestModel

                    };
GetDocumentUrls: {
                        id?: Array<string>

                    };
PostDocumentValidate: {
                        requestBody?: CreateDocumentRequestModel

                    };
GetItemDocument: {
                        id?: Array<string>

                    };
GetItemDocumentSearch: {
                        query?: string
skip?: number
take?: number

                    };
DeleteRecycleBinDocumentById: {
                        id: string

                    };
GetRecycleBinDocumentByIdOriginalParent: {
                        id: string

                    };
PutRecycleBinDocumentByIdRestore: {
                        id: string
requestBody?: MoveMediaRequestModel

                    };
GetRecycleBinDocumentChildren: {
                        parentId?: string
skip?: number
take?: number

                    };
GetRecycleBinDocumentRoot: {
                        skip?: number
take?: number

                    };
GetTreeDocumentAncestors: {
                        descendantId?: string

                    };
GetTreeDocumentChildren: {
                        dataTypeId?: string
parentId?: string
skip?: number
take?: number

                    };
GetTreeDocumentRoot: {
                        dataTypeId?: string
skip?: number
take?: number

                    };
        }


        responses: {
            GetCollectionDocumentById: PagedDocumentCollectionResponseModel
                ,PostDocument: string
                ,GetDocumentById: DocumentResponseModel
                ,DeleteDocumentById: string
                ,PutDocumentById: string
                ,GetDocumentByIdAuditLog: PagedAuditLogResponseModel
                ,PostDocumentByIdCopy: string
                ,GetDocumentByIdDomains: DomainsResponseModel
                ,PutDocumentByIdDomains: string
                ,PutDocumentByIdMove: string
                ,PutDocumentByIdMoveToRecycleBin: string
                ,GetDocumentByIdNotifications: Array<DocumentNotificationResponseModel>
                ,PutDocumentByIdNotifications: string
                ,PostDocumentByIdPublicAccess: string
                ,DeleteDocumentByIdPublicAccess: string
                ,GetDocumentByIdPublicAccess: PublicAccessResponseModel
                ,PutDocumentByIdPublicAccess: string
                ,PutDocumentByIdPublish: string
                ,PutDocumentByIdPublishWithDescendants: string
                ,GetDocumentByIdReferencedBy: PagedIReferenceResponseModel
                ,GetDocumentByIdReferencedDescendants: PagedReferenceByIdModel
                ,PutDocumentByIdUnpublish: string
                ,PutDocumentByIdValidate: string
                ,GetDocumentAreReferenced: PagedReferenceByIdModel
                ,GetDocumentConfiguration: DocumentConfigurationResponseModel
                ,PutDocumentSort: string
                ,GetDocumentUrls: Array<DocumentUrlInfoResponseModel>
                ,PostDocumentValidate: string
                ,GetItemDocument: Array<DocumentItemResponseModel>
                ,GetItemDocumentSearch: PagedModelDocumentItemResponseModel
                ,DeleteRecycleBinDocument: string
                ,DeleteRecycleBinDocumentById: string
                ,GetRecycleBinDocumentByIdOriginalParent: ReferenceByIdModel
                ,PutRecycleBinDocumentByIdRestore: string
                ,GetRecycleBinDocumentChildren: PagedDocumentRecycleBinItemResponseModel
                ,GetRecycleBinDocumentRoot: PagedDocumentRecycleBinItemResponseModel
                ,GetTreeDocumentAncestors: Array<DocumentTreeItemResponseModel>
                ,GetTreeDocumentChildren: PagedDocumentTreeItemResponseModel
                ,GetTreeDocumentRoot: PagedDocumentTreeItemResponseModel

        }

    }

export type DynamicRootData = {

        payloads: {
            PostDynamicRootQuery: {
                        requestBody?: DynamicRootRequestModel

                    };
        }


        responses: {
            PostDynamicRootQuery: DynamicRootResponseModel
                ,GetDynamicRootSteps: Array<string>

        }

    }

export type HealthCheckData = {

        payloads: {
            GetHealthCheckGroup: {
                        skip?: number
take?: number

                    };
GetHealthCheckGroupByName: {
                        name: string

                    };
PostHealthCheckGroupByNameCheck: {
                        name: string

                    };
PostHealthCheckExecuteAction: {
                        requestBody?: HealthCheckActionRequestModel

                    };
        }


        responses: {
            GetHealthCheckGroup: PagedHealthCheckGroupResponseModel
                ,GetHealthCheckGroupByName: HealthCheckGroupPresentationModel
                ,PostHealthCheckGroupByNameCheck: HealthCheckGroupWithResultResponseModel
                ,PostHealthCheckExecuteAction: HealthCheckResultResponseModel

        }

    }

export type HelpData = {

        payloads: {
            GetHelp: {
                        baseUrl?: string
section?: string
skip?: number
take?: number
tree?: string

                    };
        }


        responses: {
            GetHelp: PagedHelpPageResponseModel

        }

    }

export type ImagingData = {

        payloads: {
            GetImagingResizeUrls: {
                        height?: number
id?: Array<string>
mode?: ImageCropModeModel
width?: number

                    };
        }


        responses: {
            GetImagingResizeUrls: Array<MediaUrlInfoResponseModel>

        }

    }

export type ImportData = {

        payloads: {
            GetImportAnalyze: {
                        temporaryFileId?: string

                    };
        }


        responses: {
            GetImportAnalyze: EntityImportAnalysisResponseModel

        }

    }

export type IndexerData = {

        payloads: {
            GetIndexer: {
                        skip?: number
take?: number

                    };
GetIndexerByIndexName: {
                        indexName: string

                    };
PostIndexerByIndexNameRebuild: {
                        indexName: string

                    };
        }


        responses: {
            GetIndexer: PagedIndexResponseModel
                ,GetIndexerByIndexName: IndexResponseModel
                ,PostIndexerByIndexNameRebuild: string

        }

    }

export type InstallData = {

        payloads: {
            PostInstallSetup: {
                        requestBody?: InstallRequestModel

                    };
PostInstallValidateDatabase: {
                        requestBody?: DatabaseInstallRequestModel

                    };
        }


        responses: {
            GetInstallSettings: InstallSettingsResponseModel
                ,PostInstallSetup: string
                ,PostInstallValidateDatabase: string

        }

    }

export type LanguageData = {

        payloads: {
            GetItemLanguage: {
                        isoCode?: Array<string>

                    };
GetLanguage: {
                        skip?: number
take?: number

                    };
PostLanguage: {
                        requestBody?: CreateLanguageRequestModel

                    };
GetLanguageByIsoCode: {
                        isoCode: string

                    };
DeleteLanguageByIsoCode: {
                        isoCode: string

                    };
PutLanguageByIsoCode: {
                        isoCode: string
requestBody?: UpdateLanguageRequestModel

                    };
        }


        responses: {
            GetItemLanguage: Array<LanguageItemResponseModel>
                ,GetItemLanguageDefault: LanguageItemResponseModel
                ,GetLanguage: PagedLanguageResponseModel
                ,PostLanguage: string
                ,GetLanguageByIsoCode: LanguageResponseModel
                ,DeleteLanguageByIsoCode: string
                ,PutLanguageByIsoCode: string

        }

    }

export type LogViewerData = {

        payloads: {
            GetLogViewerLevel: {
                        skip?: number
take?: number

                    };
GetLogViewerLevelCount: {
                        endDate?: string
startDate?: string

                    };
GetLogViewerLog: {
                        endDate?: string
filterExpression?: string
logLevel?: Array<LogLevelModel>
orderDirection?: DirectionModel
skip?: number
startDate?: string
take?: number

                    };
GetLogViewerMessageTemplate: {
                        endDate?: string
skip?: number
startDate?: string
take?: number

                    };
GetLogViewerSavedSearch: {
                        skip?: number
take?: number

                    };
PostLogViewerSavedSearch: {
                        requestBody?: SavedLogSearchRequestModel

                    };
GetLogViewerSavedSearchByName: {
                        name: string

                    };
DeleteLogViewerSavedSearchByName: {
                        name: string

                    };
GetLogViewerValidateLogsSize: {
                        endDate?: string
startDate?: string

                    };
        }


        responses: {
            GetLogViewerLevel: PagedLoggerResponseModel
                ,GetLogViewerLevelCount: LogLevelCountsReponseModel
                ,GetLogViewerLog: PagedLogMessageResponseModel
                ,GetLogViewerMessageTemplate: PagedLogTemplateResponseModel
                ,GetLogViewerSavedSearch: PagedSavedLogSearchResponseModel
                ,PostLogViewerSavedSearch: string
                ,GetLogViewerSavedSearchByName: SavedLogSearchResponseModel
                ,DeleteLogViewerSavedSearchByName: string
                ,GetLogViewerValidateLogsSize: any

        }

    }

export type ManifestData = {


        responses: {
            GetManifestManifest: Array<ManifestResponseModel>
                ,GetManifestManifestPrivate: Array<ManifestResponseModel>
                ,GetManifestManifestPublic: Array<ManifestResponseModel>

        }

    }

export type MediaTypeData = {

        payloads: {
            GetItemMediaType: {
                        id?: Array<string>

                    };
GetItemMediaTypeAllowed: {
                        fileExtension?: string
skip?: number
take?: number

                    };
GetItemMediaTypeFolders: {
                        skip?: number
take?: number

                    };
GetItemMediaTypeSearch: {
                        query?: string
skip?: number
take?: number

                    };
PostMediaType: {
                        requestBody?: CreateMediaTypeRequestModel

                    };
GetMediaTypeById: {
                        id: string

                    };
DeleteMediaTypeById: {
                        id: string

                    };
PutMediaTypeById: {
                        id: string
requestBody?: UpdateMediaTypeRequestModel

                    };
GetMediaTypeByIdAllowedChildren: {
                        id: string
skip?: number
take?: number

                    };
GetMediaTypeByIdCompositionReferences: {
                        id: string

                    };
PostMediaTypeByIdCopy: {
                        id: string
requestBody?: CopyMediaTypeRequestModel

                    };
GetMediaTypeByIdExport: {
                        id: string

                    };
PutMediaTypeByIdImport: {
                        id: string
requestBody?: ImportMediaTypeRequestModel

                    };
PutMediaTypeByIdMove: {
                        id: string
requestBody?: MoveMediaTypeRequestModel

                    };
GetMediaTypeAllowedAtRoot: {
                        skip?: number
take?: number

                    };
PostMediaTypeAvailableCompositions: {
                        requestBody?: MediaTypeCompositionRequestModel

                    };
PostMediaTypeFolder: {
                        requestBody?: CreateFolderRequestModel

                    };
GetMediaTypeFolderById: {
                        id: string

                    };
DeleteMediaTypeFolderById: {
                        id: string

                    };
PutMediaTypeFolderById: {
                        id: string
requestBody?: UpdateFolderResponseModel

                    };
PostMediaTypeImport: {
                        requestBody?: ImportMediaTypeRequestModel

                    };
GetTreeMediaTypeAncestors: {
                        descendantId?: string

                    };
GetTreeMediaTypeChildren: {
                        foldersOnly?: boolean
parentId?: string
skip?: number
take?: number

                    };
GetTreeMediaTypeRoot: {
                        foldersOnly?: boolean
skip?: number
take?: number

                    };
        }


        responses: {
            GetItemMediaType: Array<MediaTypeItemResponseModel>
                ,GetItemMediaTypeAllowed: PagedModelMediaTypeItemResponseModel
                ,GetItemMediaTypeFolders: PagedModelMediaTypeItemResponseModel
                ,GetItemMediaTypeSearch: PagedModelMediaTypeItemResponseModel
                ,PostMediaType: string
                ,GetMediaTypeById: MediaTypeResponseModel
                ,DeleteMediaTypeById: string
                ,PutMediaTypeById: string
                ,GetMediaTypeByIdAllowedChildren: PagedAllowedMediaTypeModel
                ,GetMediaTypeByIdCompositionReferences: Array<MediaTypeCompositionResponseModel>
                ,PostMediaTypeByIdCopy: string
                ,GetMediaTypeByIdExport: Blob | File
                ,PutMediaTypeByIdImport: string
                ,PutMediaTypeByIdMove: string
                ,GetMediaTypeAllowedAtRoot: PagedAllowedMediaTypeModel
                ,PostMediaTypeAvailableCompositions: Array<AvailableMediaTypeCompositionResponseModel>
                ,PostMediaTypeFolder: string
                ,GetMediaTypeFolderById: FolderResponseModel
                ,DeleteMediaTypeFolderById: string
                ,PutMediaTypeFolderById: string
                ,PostMediaTypeImport: string
                ,GetTreeMediaTypeAncestors: Array<MediaTypeTreeItemResponseModel>
                ,GetTreeMediaTypeChildren: PagedMediaTypeTreeItemResponseModel
                ,GetTreeMediaTypeRoot: PagedMediaTypeTreeItemResponseModel

        }

    }

export type MediaData = {

        payloads: {
            GetCollectionMedia: {
                        dataTypeId?: string
filter?: string
id?: string
orderBy?: string
orderDirection?: DirectionModel
skip?: number
take?: number

                    };
GetItemMedia: {
                        id?: Array<string>

                    };
GetItemMediaSearch: {
                        query?: string
skip?: number
take?: number

                    };
PostMedia: {
                        requestBody?: CreateMediaRequestModel

                    };
GetMediaById: {
                        id: string

                    };
DeleteMediaById: {
                        id: string

                    };
PutMediaById: {
                        id: string
requestBody?: UpdateMediaRequestModel

                    };
GetMediaByIdAuditLog: {
                        id: string
orderDirection?: DirectionModel
sinceDate?: string
skip?: number
take?: number

                    };
PutMediaByIdMove: {
                        id: string
requestBody?: MoveMediaRequestModel

                    };
PutMediaByIdMoveToRecycleBin: {
                        id: string

                    };
GetMediaByIdReferencedBy: {
                        id: string
skip?: number
take?: number

                    };
GetMediaByIdReferencedDescendants: {
                        id: string
skip?: number
take?: number

                    };
PutMediaByIdValidate: {
                        id: string
requestBody?: UpdateMediaRequestModel

                    };
GetMediaAreReferenced: {
                        id?: Array<string>
skip?: number
take?: number

                    };
PutMediaSort: {
                        requestBody?: SortingRequestModel

                    };
GetMediaUrls: {
                        id?: Array<string>

                    };
PostMediaValidate: {
                        requestBody?: CreateMediaRequestModel

                    };
DeleteRecycleBinMediaById: {
                        id: string

                    };
GetRecycleBinMediaByIdOriginalParent: {
                        id: string

                    };
PutRecycleBinMediaByIdRestore: {
                        id: string
requestBody?: MoveMediaRequestModel

                    };
GetRecycleBinMediaChildren: {
                        parentId?: string
skip?: number
take?: number

                    };
GetRecycleBinMediaRoot: {
                        skip?: number
take?: number

                    };
GetTreeMediaAncestors: {
                        descendantId?: string

                    };
GetTreeMediaChildren: {
                        dataTypeId?: string
parentId?: string
skip?: number
take?: number

                    };
GetTreeMediaRoot: {
                        dataTypeId?: string
skip?: number
take?: number

                    };
        }


        responses: {
            GetCollectionMedia: PagedMediaCollectionResponseModel
                ,GetItemMedia: Array<MediaItemResponseModel>
                ,GetItemMediaSearch: PagedModelMediaItemResponseModel
                ,PostMedia: string
                ,GetMediaById: MediaResponseModel
                ,DeleteMediaById: string
                ,PutMediaById: string
                ,GetMediaByIdAuditLog: PagedAuditLogResponseModel
                ,PutMediaByIdMove: string
                ,PutMediaByIdMoveToRecycleBin: string
                ,GetMediaByIdReferencedBy: PagedIReferenceResponseModel
                ,GetMediaByIdReferencedDescendants: PagedReferenceByIdModel
                ,PutMediaByIdValidate: string
                ,GetMediaAreReferenced: PagedReferenceByIdModel
                ,GetMediaConfiguration: MediaConfigurationResponseModel
                ,PutMediaSort: string
                ,GetMediaUrls: Array<MediaUrlInfoResponseModel>
                ,PostMediaValidate: string
                ,DeleteRecycleBinMedia: string
                ,DeleteRecycleBinMediaById: string
                ,GetRecycleBinMediaByIdOriginalParent: ReferenceByIdModel
                ,PutRecycleBinMediaByIdRestore: string
                ,GetRecycleBinMediaChildren: PagedMediaRecycleBinItemResponseModel
                ,GetRecycleBinMediaRoot: PagedMediaRecycleBinItemResponseModel
                ,GetTreeMediaAncestors: Array<MediaTreeItemResponseModel>
                ,GetTreeMediaChildren: PagedMediaTreeItemResponseModel
                ,GetTreeMediaRoot: PagedMediaTreeItemResponseModel

        }

    }

export type MemberGroupData = {

        payloads: {
            GetItemMemberGroup: {
                        id?: Array<string>

                    };
GetMemberGroup: {
                        skip?: number
take?: number

                    };
PostMemberGroup: {
                        requestBody?: CreateMemberGroupRequestModel

                    };
GetMemberGroupById: {
                        id: string

                    };
DeleteMemberGroupById: {
                        id: string

                    };
PutMemberGroupById: {
                        id: string
requestBody?: UpdateMemberGroupRequestModel

                    };
GetTreeMemberGroupRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemMemberGroup: Array<MemberGroupItemResponseModel>
                ,GetMemberGroup: PagedMemberGroupResponseModel
                ,PostMemberGroup: string
                ,GetMemberGroupById: MemberGroupResponseModel
                ,DeleteMemberGroupById: string
                ,PutMemberGroupById: string
                ,GetTreeMemberGroupRoot: PagedNamedEntityTreeItemResponseModel

        }

    }

export type MemberTypeData = {

        payloads: {
            GetItemMemberType: {
                        id?: Array<string>

                    };
GetItemMemberTypeSearch: {
                        query?: string
skip?: number
take?: number

                    };
PostMemberType: {
                        requestBody?: CreateMemberTypeRequestModel

                    };
GetMemberTypeById: {
                        id: string

                    };
DeleteMemberTypeById: {
                        id: string

                    };
PutMemberTypeById: {
                        id: string
requestBody?: UpdateMemberTypeRequestModel

                    };
GetMemberTypeByIdCompositionReferences: {
                        id: string

                    };
PostMemberTypeByIdCopy: {
                        id: string

                    };
PostMemberTypeAvailableCompositions: {
                        requestBody?: MemberTypeCompositionRequestModel

                    };
GetTreeMemberTypeRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemMemberType: Array<MemberTypeItemResponseModel>
                ,GetItemMemberTypeSearch: PagedModelMemberTypeItemResponseModel
                ,PostMemberType: string
                ,GetMemberTypeById: MemberTypeResponseModel
                ,DeleteMemberTypeById: string
                ,PutMemberTypeById: string
                ,GetMemberTypeByIdCompositionReferences: Array<MemberTypeCompositionResponseModel>
                ,PostMemberTypeByIdCopy: string
                ,PostMemberTypeAvailableCompositions: Array<AvailableMemberTypeCompositionResponseModel>
                ,GetTreeMemberTypeRoot: PagedMemberTypeTreeItemResponseModel

        }

    }

export type MemberData = {

        payloads: {
            GetFilterMember: {
                        filter?: string
isApproved?: boolean
isLockedOut?: boolean
memberGroupName?: string
memberTypeId?: string
orderBy?: string
orderDirection?: DirectionModel
skip?: number
take?: number

                    };
GetItemMember: {
                        id?: Array<string>

                    };
GetItemMemberSearch: {
                        query?: string
skip?: number
take?: number

                    };
PostMember: {
                        requestBody?: CreateMemberRequestModel

                    };
GetMemberById: {
                        id: string

                    };
DeleteMemberById: {
                        id: string

                    };
PutMemberById: {
                        id: string
requestBody?: UpdateMemberRequestModel

                    };
PutMemberByIdValidate: {
                        id: string
requestBody?: UpdateMemberRequestModel

                    };
PostMemberValidate: {
                        requestBody?: CreateMemberRequestModel

                    };
        }


        responses: {
            GetFilterMember: PagedMemberResponseModel
                ,GetItemMember: Array<MemberItemResponseModel>
                ,GetItemMemberSearch: PagedModelMemberItemResponseModel
                ,PostMember: string
                ,GetMemberById: MemberResponseModel
                ,DeleteMemberById: string
                ,PutMemberById: string
                ,PutMemberByIdValidate: string
                ,GetMemberConfiguration: MemberConfigurationResponseModel
                ,PostMemberValidate: string

        }

    }

export type ModelsBuilderData = {


        responses: {
            PostModelsBuilderBuild: string
                ,GetModelsBuilderDashboard: ModelsBuilderResponseModel
                ,GetModelsBuilderStatus: OutOfDateStatusResponseModel

        }

    }

export type ObjectTypesData = {

        payloads: {
            GetObjectTypes: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetObjectTypes: PagedObjectTypeResponseModel

        }

    }

export type OembedData = {

        payloads: {
            GetOembedQuery: {
                        maxHeight?: number
maxWidth?: number
url?: string

                    };
        }


        responses: {
            GetOembedQuery: OEmbedResponseModel

        }

    }

export type PackageData = {

        payloads: {
            PostPackageByNameRunMigration: {
                        name: string

                    };
GetPackageCreated: {
                        skip?: number
take?: number

                    };
PostPackageCreated: {
                        requestBody?: CreatePackageRequestModel

                    };
GetPackageCreatedById: {
                        id: string

                    };
DeletePackageCreatedById: {
                        id: string

                    };
PutPackageCreatedById: {
                        id: string
requestBody?: UpdatePackageRequestModel

                    };
GetPackageCreatedByIdDownload: {
                        id: string

                    };
GetPackageMigrationStatus: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            PostPackageByNameRunMigration: string
                ,GetPackageConfiguration: PackageConfigurationResponseModel
                ,GetPackageCreated: PagedPackageDefinitionResponseModel
                ,PostPackageCreated: string
                ,GetPackageCreatedById: PackageDefinitionResponseModel
                ,DeletePackageCreatedById: string
                ,PutPackageCreatedById: string
                ,GetPackageCreatedByIdDownload: Blob | File
                ,GetPackageMigrationStatus: PagedPackageMigrationStatusResponseModel

        }

    }

export type PartialViewData = {

        payloads: {
            GetItemPartialView: {
                        path?: Array<string>

                    };
PostPartialView: {
                        requestBody?: CreatePartialViewRequestModel

                    };
GetPartialViewByPath: {
                        path: string

                    };
DeletePartialViewByPath: {
                        path: string

                    };
PutPartialViewByPath: {
                        path: string
requestBody?: UpdatePartialViewRequestModel

                    };
PutPartialViewByPathRename: {
                        path: string
requestBody?: RenamePartialViewRequestModel

                    };
PostPartialViewFolder: {
                        requestBody?: CreatePartialViewFolderRequestModel

                    };
GetPartialViewFolderByPath: {
                        path: string

                    };
DeletePartialViewFolderByPath: {
                        path: string

                    };
GetPartialViewSnippet: {
                        skip?: number
take?: number

                    };
GetPartialViewSnippetById: {
                        id: string

                    };
GetTreePartialViewAncestors: {
                        descendantPath?: string

                    };
GetTreePartialViewChildren: {
                        parentPath?: string
skip?: number
take?: number

                    };
GetTreePartialViewRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemPartialView: Array<PartialViewItemResponseModel>
                ,PostPartialView: string
                ,GetPartialViewByPath: PartialViewResponseModel
                ,DeletePartialViewByPath: string
                ,PutPartialViewByPath: string
                ,PutPartialViewByPathRename: string
                ,PostPartialViewFolder: string
                ,GetPartialViewFolderByPath: PartialViewFolderResponseModel
                ,DeletePartialViewFolderByPath: string
                ,GetPartialViewSnippet: PagedPartialViewSnippetItemResponseModel
                ,GetPartialViewSnippetById: PartialViewSnippetResponseModel
                ,GetTreePartialViewAncestors: Array<FileSystemTreeItemPresentationModel>
                ,GetTreePartialViewChildren: PagedFileSystemTreeItemPresentationModel
                ,GetTreePartialViewRoot: PagedFileSystemTreeItemPresentationModel

        }

    }

export type PreviewData = {


        responses: {
            DeletePreview: string
                ,PostPreview: string

        }

    }

export type ProfilingData = {

        payloads: {
            PutProfilingStatus: {
                        requestBody?: ProfilingStatusRequestModel

                    };
        }


        responses: {
            GetProfilingStatus: ProfilingStatusResponseModel
                ,PutProfilingStatus: string

        }

    }

export type PropertyTypeData = {

        payloads: {
            GetPropertyTypeIsUsed: {
                        contentTypeId?: string
propertyAlias?: string

                    };
        }


        responses: {
            GetPropertyTypeIsUsed: boolean

        }

    }

export type PublishedCacheData = {


        responses: {
            PostPublishedCacheCollect: string
                ,PostPublishedCacheRebuild: string
                ,PostPublishedCacheReload: string
                ,GetPublishedCacheStatus: string

        }

    }

export type RedirectManagementData = {

        payloads: {
            GetRedirectManagement: {
                        filter?: string
skip?: number
take?: number

                    };
GetRedirectManagementById: {
                        id: string
skip?: number
take?: number

                    };
DeleteRedirectManagementById: {
                        id: string

                    };
PostRedirectManagementStatus: {
                        status?: RedirectStatusModel

                    };
        }


        responses: {
            GetRedirectManagement: PagedRedirectUrlResponseModel
                ,GetRedirectManagementById: PagedRedirectUrlResponseModel
                ,DeleteRedirectManagementById: string
                ,GetRedirectManagementStatus: RedirectUrlStatusResponseModel
                ,PostRedirectManagementStatus: string

        }

    }

export type RelationTypeData = {

        payloads: {
            GetItemRelationType: {
                        id?: Array<string>

                    };
GetRelationType: {
                        skip?: number
take?: number

                    };
GetRelationTypeById: {
                        id: string

                    };
        }


        responses: {
            GetItemRelationType: Array<RelationTypeItemResponseModel>
                ,GetRelationType: PagedRelationTypeResponseModel
                ,GetRelationTypeById: RelationTypeResponseModel

        }

    }

export type RelationData = {

        payloads: {
            GetRelationTypeById: {
                        id: string
skip?: number
take?: number

                    };
        }


        responses: {
            GetRelationTypeById: PagedRelationResponseModel

        }

    }

export type ScriptData = {

        payloads: {
            GetItemScript: {
                        path?: Array<string>

                    };
PostScript: {
                        requestBody?: CreateScriptRequestModel

                    };
GetScriptByPath: {
                        path: string

                    };
DeleteScriptByPath: {
                        path: string

                    };
PutScriptByPath: {
                        path: string
requestBody?: UpdateScriptRequestModel

                    };
PutScriptByPathRename: {
                        path: string
requestBody?: RenameScriptRequestModel

                    };
PostScriptFolder: {
                        requestBody?: CreateScriptFolderRequestModel

                    };
GetScriptFolderByPath: {
                        path: string

                    };
DeleteScriptFolderByPath: {
                        path: string

                    };
GetTreeScriptAncestors: {
                        descendantPath?: string

                    };
GetTreeScriptChildren: {
                        parentPath?: string
skip?: number
take?: number

                    };
GetTreeScriptRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemScript: Array<ScriptItemResponseModel>
                ,PostScript: string
                ,GetScriptByPath: ScriptResponseModel
                ,DeleteScriptByPath: string
                ,PutScriptByPath: string
                ,PutScriptByPathRename: string
                ,PostScriptFolder: string
                ,GetScriptFolderByPath: ScriptFolderResponseModel
                ,DeleteScriptFolderByPath: string
                ,GetTreeScriptAncestors: Array<FileSystemTreeItemPresentationModel>
                ,GetTreeScriptChildren: PagedFileSystemTreeItemPresentationModel
                ,GetTreeScriptRoot: PagedFileSystemTreeItemPresentationModel

        }

    }

export type SearcherData = {

        payloads: {
            GetSearcher: {
                        skip?: number
take?: number

                    };
GetSearcherBySearcherNameQuery: {
                        searcherName: string
skip?: number
take?: number
term?: string

                    };
        }


        responses: {
            GetSearcher: PagedSearcherResponseModel
                ,GetSearcherBySearcherNameQuery: PagedSearchResultResponseModel

        }

    }

export type SecurityData = {

        payloads: {
            PostSecurityForgotPassword: {
                        requestBody?: ResetPasswordRequestModel

                    };
PostSecurityForgotPasswordReset: {
                        requestBody?: ResetPasswordTokenRequestModel

                    };
PostSecurityForgotPasswordVerify: {
                        requestBody?: VerifyResetPasswordTokenRequestModel

                    };
        }


        responses: {
            GetSecurityConfiguration: SecurityConfigurationResponseModel
                ,PostSecurityForgotPassword: string
                ,PostSecurityForgotPasswordReset: string
                ,PostSecurityForgotPasswordVerify: VerifyResetPasswordResponseModel

        }

    }

export type SegmentData = {

        payloads: {
            GetSegment: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetSegment: PagedSegmentResponseModel

        }

    }

export type ServerData = {


        responses: {
            GetServerConfiguration: ServerConfigurationResponseModel
                ,GetServerInformation: ServerInformationResponseModel
                ,GetServerStatus: ServerStatusResponseModel
                ,GetServerTroubleshooting: ServerTroubleshootingResponseModel

        }

    }

export type StaticFileData = {

        payloads: {
            GetItemStaticFile: {
                        path?: Array<string>

                    };
GetTreeStaticFileAncestors: {
                        descendantPath?: string

                    };
GetTreeStaticFileChildren: {
                        parentPath?: string
skip?: number
take?: number

                    };
GetTreeStaticFileRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemStaticFile: Array<StaticFileItemResponseModel>
                ,GetTreeStaticFileAncestors: Array<FileSystemTreeItemPresentationModel>
                ,GetTreeStaticFileChildren: PagedFileSystemTreeItemPresentationModel
                ,GetTreeStaticFileRoot: PagedFileSystemTreeItemPresentationModel

        }

    }

export type StylesheetData = {

        payloads: {
            GetItemStylesheet: {
                        path?: Array<string>

                    };
PostStylesheet: {
                        requestBody?: CreateStylesheetRequestModel

                    };
GetStylesheetByPath: {
                        path: string

                    };
DeleteStylesheetByPath: {
                        path: string

                    };
PutStylesheetByPath: {
                        path: string
requestBody?: UpdateStylesheetRequestModel

                    };
PutStylesheetByPathRename: {
                        path: string
requestBody?: RenameStylesheetRequestModel

                    };
PostStylesheetFolder: {
                        requestBody?: CreateStylesheetFolderRequestModel

                    };
GetStylesheetFolderByPath: {
                        path: string

                    };
DeleteStylesheetFolderByPath: {
                        path: string

                    };
GetTreeStylesheetAncestors: {
                        descendantPath?: string

                    };
GetTreeStylesheetChildren: {
                        parentPath?: string
skip?: number
take?: number

                    };
GetTreeStylesheetRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemStylesheet: Array<StylesheetItemResponseModel>
                ,PostStylesheet: string
                ,GetStylesheetByPath: StylesheetResponseModel
                ,DeleteStylesheetByPath: string
                ,PutStylesheetByPath: string
                ,PutStylesheetByPathRename: string
                ,PostStylesheetFolder: string
                ,GetStylesheetFolderByPath: StylesheetFolderResponseModel
                ,DeleteStylesheetFolderByPath: string
                ,GetTreeStylesheetAncestors: Array<FileSystemTreeItemPresentationModel>
                ,GetTreeStylesheetChildren: PagedFileSystemTreeItemPresentationModel
                ,GetTreeStylesheetRoot: PagedFileSystemTreeItemPresentationModel

        }

    }

export type TagData = {

        payloads: {
            GetTag: {
                        culture?: string
query?: string
skip?: number
tagGroup?: string
take?: number

                    };
        }


        responses: {
            GetTag: PagedTagResponseModel

        }

    }

export type TelemetryData = {

        payloads: {
            GetTelemetry: {
                        skip?: number
take?: number

                    };
PostTelemetryLevel: {
                        requestBody?: TelemetryRequestModel

                    };
        }


        responses: {
            GetTelemetry: PagedTelemetryResponseModel
                ,GetTelemetryLevel: TelemetryResponseModel
                ,PostTelemetryLevel: string

        }

    }

export type TemplateData = {

        payloads: {
            GetItemTemplate: {
                        id?: Array<string>

                    };
GetItemTemplateSearch: {
                        query?: string
skip?: number
take?: number

                    };
PostTemplate: {
                        requestBody?: CreateTemplateRequestModel

                    };
GetTemplateById: {
                        id: string

                    };
DeleteTemplateById: {
                        id: string

                    };
PutTemplateById: {
                        id: string
requestBody?: UpdateTemplateRequestModel

                    };
PostTemplateQueryExecute: {
                        requestBody?: TemplateQueryExecuteModel

                    };
GetTreeTemplateAncestors: {
                        descendantId?: string

                    };
GetTreeTemplateChildren: {
                        parentId?: string
skip?: number
take?: number

                    };
GetTreeTemplateRoot: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemTemplate: Array<TemplateItemResponseModel>
                ,GetItemTemplateSearch: PagedModelTemplateItemResponseModel
                ,PostTemplate: string
                ,GetTemplateById: TemplateResponseModel
                ,DeleteTemplateById: string
                ,PutTemplateById: string
                ,GetTemplateConfiguration: TemplateConfigurationResponseModel
                ,PostTemplateQueryExecute: TemplateQueryResultResponseModel
                ,GetTemplateQuerySettings: TemplateQuerySettingsResponseModel
                ,GetTreeTemplateAncestors: Array<NamedEntityTreeItemResponseModel>
                ,GetTreeTemplateChildren: PagedNamedEntityTreeItemResponseModel
                ,GetTreeTemplateRoot: PagedNamedEntityTreeItemResponseModel

        }

    }

export type TemporaryFileData = {

        payloads: {
            PostTemporaryFile: {
                        formData?: {
        Id: string
File: Blob | File
    }

                    };
GetTemporaryFileById: {
                        id: string

                    };
DeleteTemporaryFileById: {
                        id: string

                    };
        }


        responses: {
            PostTemporaryFile: string
                ,GetTemporaryFileById: TemporaryFileResponseModel
                ,DeleteTemporaryFileById: string
                ,GetTemporaryFileConfiguration: TemporaryFileConfigurationResponseModel

        }

    }

export type UpgradeData = {


        responses: {
            PostUpgradeAuthorize: string
                ,GetUpgradeSettings: UpgradeSettingsResponseModel

        }

    }

export type UserDataData = {

        payloads: {
            PostUserData: {
                        requestBody?: CreateUserDataRequestModel

                    };
GetUserData: {
                        groups?: Array<string>
identifiers?: Array<string>
skip?: number
take?: number

                    };
PutUserData: {
                        requestBody?: UpdateUserDataRequestModel

                    };
GetUserDataById: {
                        id: string

                    };
        }


        responses: {
            PostUserData: string
                ,GetUserData: PagedUserDataResponseModel
                ,PutUserData: string
                ,GetUserDataById: UserDataModel

        }

    }

export type UserGroupData = {

        payloads: {
            GetFilterUserGroup: {
                        filter?: string
skip?: number
take?: number

                    };
GetItemUserGroup: {
                        id?: Array<string>

                    };
DeleteUserGroup: {
                        requestBody?: DeleteUserGroupsRequestModel

                    };
PostUserGroup: {
                        requestBody?: CreateUserGroupRequestModel

                    };
GetUserGroup: {
                        skip?: number
take?: number

                    };
GetUserGroupById: {
                        id: string

                    };
DeleteUserGroupById: {
                        id: string

                    };
PutUserGroupById: {
                        id: string
requestBody?: UpdateUserGroupRequestModel

                    };
DeleteUserGroupByIdUsers: {
                        id: string
requestBody?: Array<ReferenceByIdModel>

                    };
PostUserGroupByIdUsers: {
                        id: string
requestBody?: Array<ReferenceByIdModel>

                    };
        }


        responses: {
            GetFilterUserGroup: PagedUserGroupResponseModel
                ,GetItemUserGroup: Array<UserGroupItemResponseModel>
                ,DeleteUserGroup: string
                ,PostUserGroup: string
                ,GetUserGroup: PagedUserGroupResponseModel
                ,GetUserGroupById: UserGroupResponseModel
                ,DeleteUserGroupById: string
                ,PutUserGroupById: string
                ,DeleteUserGroupByIdUsers: string
                ,PostUserGroupByIdUsers: string

        }

    }

export type UserData = {

        payloads: {
            GetFilterUser: {
                        filter?: string
orderBy?: UserOrderModel
orderDirection?: DirectionModel
skip?: number
take?: number
userGroupIds?: Array<string>
userStates?: Array<UserStateModel>

                    };
GetItemUser: {
                        id?: Array<string>

                    };
PostUser: {
                        requestBody?: CreateUserRequestModel

                    };
DeleteUser: {
                        requestBody?: DeleteUsersRequestModel

                    };
GetUser: {
                        skip?: number
take?: number

                    };
GetUserById: {
                        id: string

                    };
DeleteUserById: {
                        id: string

                    };
PutUserById: {
                        id: string
requestBody?: UpdateUserRequestModel

                    };
GetUserById2Fa: {
                        id: string

                    };
DeleteUserById2FaByProviderName: {
                        id: string
providerName: string

                    };
PostUserByIdChangePassword: {
                        id: string
requestBody?: ChangePasswordUserRequestModel

                    };
PostUserByIdResetPassword: {
                        id: string

                    };
DeleteUserAvatarById: {
                        id: string

                    };
PostUserAvatarById: {
                        id: string
requestBody?: SetAvatarRequestModel

                    };
DeleteUserCurrent2FaByProviderName: {
                        code?: string
providerName: string

                    };
PostUserCurrent2FaByProviderName: {
                        providerName: string
requestBody?: EnableTwoFactorRequestModel

                    };
GetUserCurrent2FaByProviderName: {
                        providerName: string

                    };
PostUserCurrentAvatar: {
                        requestBody?: SetAvatarRequestModel

                    };
PostUserCurrentChangePassword: {
                        requestBody?: ChangePasswordCurrentUserRequestModel

                    };
GetUserCurrentPermissions: {
                        id?: Array<string>

                    };
GetUserCurrentPermissionsDocument: {
                        id?: Array<string>

                    };
GetUserCurrentPermissionsMedia: {
                        id?: Array<string>

                    };
PostUserDisable: {
                        requestBody?: DisableUserRequestModel

                    };
PostUserEnable: {
                        requestBody?: EnableUserRequestModel

                    };
PostUserInvite: {
                        requestBody?: InviteUserRequestModel

                    };
PostUserInviteCreatePassword: {
                        requestBody?: CreateInitialPasswordUserRequestModel

                    };
PostUserInviteResend: {
                        requestBody?: ResendInviteUserRequestModel

                    };
PostUserInviteVerify: {
                        requestBody?: VerifyInviteUserRequestModel

                    };
PostUserSetUserGroups: {
                        requestBody?: UpdateUserGroupsOnUserRequestModel

                    };
PostUserUnlock: {
                        requestBody?: UnlockUsersRequestModel

                    };
        }


        responses: {
            GetFilterUser: PagedUserResponseModel
                ,GetItemUser: Array<UserItemResponseModel>
                ,PostUser: string
                ,DeleteUser: string
                ,GetUser: PagedUserResponseModel
                ,GetUserById: UserResponseModel
                ,DeleteUserById: string
                ,PutUserById: string
                ,GetUserById2Fa: Array<UserTwoFactorProviderModel>
                ,DeleteUserById2FaByProviderName: string
                ,PostUserByIdChangePassword: string
                ,PostUserByIdResetPassword: ResetPasswordUserResponseModel
                ,DeleteUserAvatarById: string
                ,PostUserAvatarById: string
                ,GetUserConfiguration: UserConfigurationResponseModel
                ,GetUserCurrent: CurrentUserResponseModel
                ,GetUserCurrent2Fa: Array<UserTwoFactorProviderModel>
                ,DeleteUserCurrent2FaByProviderName: string
                ,PostUserCurrent2FaByProviderName: NoopSetupTwoFactorModel
                ,GetUserCurrent2FaByProviderName: NoopSetupTwoFactorModel
                ,PostUserCurrentAvatar: string
                ,PostUserCurrentChangePassword: string
                ,GetUserCurrentConfiguration: CurrenUserConfigurationResponseModel
                ,GetUserCurrentLoginProviders: Array<UserExternalLoginProviderModel>
                ,GetUserCurrentPermissions: UserPermissionsResponseModel
                ,GetUserCurrentPermissionsDocument: Array<UserPermissionsResponseModel>
                ,GetUserCurrentPermissionsMedia: UserPermissionsResponseModel
                ,PostUserDisable: string
                ,PostUserEnable: string
                ,PostUserInvite: string
                ,PostUserInviteCreatePassword: string
                ,PostUserInviteResend: string
                ,PostUserInviteVerify: VerifyInviteUserResponseModel
                ,PostUserSetUserGroups: string
                ,PostUserUnlock: string

        }

    }

export type WebhookData = {

        payloads: {
            GetItemWebhook: {
                        id?: Array<string>

                    };
GetWebhook: {
                        skip?: number
take?: number

                    };
PostWebhook: {
                        requestBody?: CreateWebhookRequestModel

                    };
GetWebhookById: {
                        id: string

                    };
DeleteWebhookById: {
                        id: string

                    };
PutWebhookById: {
                        id: string
requestBody?: UpdateWebhookRequestModel

                    };
GetWebhookEvents: {
                        skip?: number
take?: number

                    };
        }


        responses: {
            GetItemWebhook: Array<WebhookItemResponseModel>
                ,GetWebhook: PagedWebhookResponseModel
                ,PostWebhook: string
                ,GetWebhookById: WebhookResponseModel
                ,DeleteWebhookById: string
                ,PutWebhookById: string
                ,GetWebhookEvents: PagedWebhookEventModel

        }

    }
