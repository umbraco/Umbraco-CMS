import { UMB_PARTIAL_VIEW_ENTITY_TYPE } from './entity.js';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_PARTIAL_VIEW_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_PARTIAL_VIEW_ENTITY_TYPE,
});

export const UMB_CREATE_PARTIAL_VIEW_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbEntityModel['entityType'];
	parentUnique: UmbEntityModel['unique'];
}>('create/parent/:parentEntityType/:parentUnique', UMB_PARTIAL_VIEW_WORKSPACE_PATH);

export const UMB_EDIT_PARTIAL_VIEW_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>(
	'edit/:unique',
	UMB_PARTIAL_VIEW_WORKSPACE_PATH,
);
