import { UMB_MEDIA_SECTION_PATHNAME } from '../media-section/paths.js';
import { UMB_MEDIA_ENTITY_TYPE, type UmbMediaEntityTypeUnion } from './entity.js';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';

export const UMB_MEDIA_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEDIA_SECTION_PATHNAME,
	entityType: UMB_MEDIA_ENTITY_TYPE,
});

export const UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{
	parentEntityType: UmbMediaEntityTypeUnion;
	parentUnique?: string | null;
	mediaTypeUnique: string;
}>('create/parent/:parentEntityType/:parentUnique/:mediaTypeUnique', UMB_MEDIA_WORKSPACE_PATH);

export const UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>('edit/:unique');
