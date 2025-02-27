import { UMB_USER_SECTION_PATHNAME } from '../section/paths.js';
import { UMB_USER_GROUP_ENTITY_TYPE, UMB_USER_GROUP_ROOT_ENTITY_TYPE } from './entity.js';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_USER_GROUP_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_USER_SECTION_PATHNAME,
	entityType: UMB_USER_GROUP_ENTITY_TYPE,
});

export const UMB_USER_GROUP_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_USER_SECTION_PATHNAME,
	entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE,
});
