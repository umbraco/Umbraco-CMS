export const UMB_DOCUMENT_ENTITY_TYPE = 'document';
export const UMB_DOCUMENT_ROOT_ENTITY_TYPE = 'document-root';

export type UmbDocumentEntityType = typeof UMB_DOCUMENT_ENTITY_TYPE;
export type UmbDocumentRootEntityType = typeof UMB_DOCUMENT_ROOT_ENTITY_TYPE;

export type UmbDocumentEntityTypeUnion = UmbDocumentEntityType | UmbDocumentRootEntityType;
