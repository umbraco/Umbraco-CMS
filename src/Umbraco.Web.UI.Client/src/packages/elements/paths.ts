import { UMB_ELEMENT_ENTITY_TYPE } from './entity.js';
import type { UmbElementEntityTypeUnion } from './entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UMB_LIBRARY_SECTION_PATHNAME } from '@umbraco-cms/backoffice/library';

export const UMB_ELEMENT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_LIBRARY_SECTION_PATHNAME,
	entityType: UMB_ELEMENT_ENTITY_TYPE,
});

export const UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbElementEntityTypeUnion;
	parentUnique?: string | null;
	documentTypeUnique: string;
}>('create/parent/:parentEntityType/:parentUnique/:documentTypeUnique', UMB_ELEMENT_WORKSPACE_PATH);

export const UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_ELEMENT_WORKSPACE_PATH,
);
