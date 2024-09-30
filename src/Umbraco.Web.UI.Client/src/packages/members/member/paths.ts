import { UMB_MEMBER_MANAGEMENT_SECTION_PATHNAME } from '../section/paths.js';
import { UMB_MEMBER_ENTITY_TYPE, UMB_MEMBER_ROOT_ENTITY_TYPE } from './entity.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEMBER_MANAGEMENT_SECTION_PATHNAME,
	entityType: UMB_MEMBER_ENTITY_TYPE,
});

export const UMB_MEMBER_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEMBER_MANAGEMENT_SECTION_PATHNAME,
	entityType: UMB_MEMBER_ROOT_ENTITY_TYPE,
});

export const UMB_CREATE_MEMBER_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	memberTypeUnique: string;
}>('create/:memberTypeUnique', UMB_MEMBER_WORKSPACE_PATH);
