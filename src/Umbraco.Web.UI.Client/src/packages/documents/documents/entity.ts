export const UMB_DOCUMENT_ENTITY_TYPE = 'document';
export const UMB_DOCUMENT_ROOT_ENTITY_TYPE = 'document-root';

export type UmbDocumentEntityType = typeof UMB_DOCUMENT_ENTITY_TYPE;
export type UmbDocumentRootEntityType = typeof UMB_DOCUMENT_ROOT_ENTITY_TYPE;

export type UmbDocumentEntityTypeUnion = UmbDocumentEntityType | UmbDocumentRootEntityType;

// TODO: move this to a better location inside the document module
export const UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE = `${UMB_DOCUMENT_ENTITY_TYPE}-property-value`;
export type UmbDocumentPropertyValueEntityType = typeof UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE;
