import { UMB_SEARCH_INDEX_ENTITY_TYPE, UMB_SEARCH_ROOT_ENTITY_TYPE } from '../constants.js';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';

export const UMB_SEARCH_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_SEARCH_ROOT_ENTITY_TYPE,
});

export const UMB_SEARCH_INDEX_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_SEARCH_INDEX_ENTITY_TYPE,
});

export const UMB_EDIT_SEARCH_INDEX_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_SEARCH_INDEX_WORKSPACE_PATH,
);
