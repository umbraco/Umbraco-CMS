import { UMB_LANGUAGE_ENTITY_TYPE } from './entity.js';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_LANGUAGE_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_LANGUAGE_ENTITY_TYPE,
});

export const UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN = new UmbPathPattern('create', UMB_LANGUAGE_WORKSPACE_PATH);

export const UMB_EDIT_LANGUAGE_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_LANGUAGE_WORKSPACE_PATH,
);
