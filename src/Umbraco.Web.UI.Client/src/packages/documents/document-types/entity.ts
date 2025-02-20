import type { UmbDocumentTypeFolderEntityType } from './tree/index.js';

export const UMB_DOCUMENT_TYPE_ENTITY_TYPE = 'document-type';
export const UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE = 'document-type-root';

export type UmbDocumentTypeEntityType = typeof UMB_DOCUMENT_TYPE_ENTITY_TYPE;
export type UmbDocumentTypeRootEntityType = typeof UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE;

export type UmbDocumentTypeEntityTypeUnion =
	| UmbDocumentTypeEntityType
	| UmbDocumentTypeRootEntityType
	| UmbDocumentTypeFolderEntityType;
