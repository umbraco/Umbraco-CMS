import { UMB_PROPERTY_TYPE_ENTITY_TYPE } from './constants.js';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';

export const UMB_PROPERTY_TYPE_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_PROPERTY_TYPE_ENTITY_TYPE,
});

export const UMB_CREATE_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	containerUnique: string;
}>('create/:containerUnique', UMB_PROPERTY_TYPE_WORKSPACE_PATH);

export const UMB_EDIT_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>('edit/:unique');
