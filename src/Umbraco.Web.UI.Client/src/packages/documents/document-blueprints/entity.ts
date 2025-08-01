export const UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE = 'document-blueprint-root';
export const UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE = 'document-blueprint';
export const UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE = 'document-blueprint-folder';

export type UmbDocumentBlueprintRootEntityType = typeof UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE;
export type UmbDocumentBlueprintEntityType = typeof UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE;
export type UmbDocumentBlueprintFolderEntityType = typeof UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE;

export const UMB_DOCUMENT_BLUEPRINT_PROPERTY_VALUE_ENTITY_TYPE = `${UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE}-property-value`;
export type UmbDocumentBlueprintPropertyValueEntityType = typeof UMB_DOCUMENT_BLUEPRINT_PROPERTY_VALUE_ENTITY_TYPE;

export type UmbDocumentBlueprintEntityTypeUnion =
	| UmbDocumentBlueprintRootEntityType
	| UmbDocumentBlueprintEntityType
	| UmbDocumentBlueprintFolderEntityType;
