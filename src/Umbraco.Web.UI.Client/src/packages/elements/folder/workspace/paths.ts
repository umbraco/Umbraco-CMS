import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_LIBRARY_SECTION_PATHNAME } from '@umbraco-cms/backoffice/library';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_ELEMENT_FOLDER_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_LIBRARY_SECTION_PATHNAME,
	entityType: UMB_ELEMENT_FOLDER_ENTITY_TYPE,
});

export const UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_ELEMENT_FOLDER_WORKSPACE_PATH,
);
