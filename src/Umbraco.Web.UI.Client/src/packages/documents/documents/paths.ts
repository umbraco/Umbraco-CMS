import { UMB_DOCUMENTS_SECTION_PATHNAME } from '../section/paths.js';
import { UMB_DOCUMENT_ENTITY_TYPE, type UmbDocumentEntityTypeUnion } from './entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_DOCUMENT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_DOCUMENTS_SECTION_PATHNAME,
	entityType: UMB_DOCUMENT_ENTITY_TYPE,
});

export const UMB_CREATE_FROM_BLUEPRINT_DOCUMENT_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbDocumentEntityTypeUnion;
	parentUnique?: string | null;
	documentTypeUnique: string;
	blueprintUnique: string;
}>(
	'create/parent/:parentEntityType/:parentUnique/:documentTypeUnique/blueprint/:blueprintUnique',
	UMB_DOCUMENT_WORKSPACE_PATH,
);

export const UMB_CREATE_DOCUMENT_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbDocumentEntityTypeUnion;
	parentUnique?: string | null;
	documentTypeUnique: string;
}>('create/parent/:parentEntityType/:parentUnique/:documentTypeUnique', UMB_DOCUMENT_WORKSPACE_PATH);

export const UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_DOCUMENT_WORKSPACE_PATH,
);
