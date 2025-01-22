import { UMB_DATA_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_DATA_TYPE_FOLDER_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_DATA_TYPE_FOLDER_ENTITY_TYPE,
});

export const UMB_EDIT_DATA_TYPE_FOLDER_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_DATA_TYPE_FOLDER_WORKSPACE_PATH,
);
