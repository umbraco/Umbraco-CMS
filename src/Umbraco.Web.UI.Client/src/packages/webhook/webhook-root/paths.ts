import { UMB_WEBHOOK_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_WEBHOOK_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE,
});
