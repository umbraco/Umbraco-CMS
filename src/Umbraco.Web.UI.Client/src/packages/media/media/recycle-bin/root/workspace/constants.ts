import { UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_SECTION_PATHNAME } from '../../../../media-section/paths.js';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEDIA_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Media.RecycleBin.Root';

export const UMB_MEDIA_RECYCLE_BIN_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEDIA_SECTION_PATHNAME,
	entityType: UMB_MEDIA_RECYCLE_BIN_ROOT_ENTITY_TYPE,
});
