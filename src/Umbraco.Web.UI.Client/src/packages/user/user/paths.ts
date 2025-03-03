import { UMB_USER_SECTION_PATHNAME } from '../section/paths.js';
import { UMB_USER_ENTITY_TYPE, UMB_USER_ROOT_ENTITY_TYPE } from './entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_USER_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_USER_SECTION_PATHNAME,
	entityType: UMB_USER_ENTITY_TYPE,
});

export const UMB_USER_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_USER_SECTION_PATHNAME,
	entityType: UMB_USER_ROOT_ENTITY_TYPE,
});

export const UMB_EDIT_USER_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>('edit/:unique');
