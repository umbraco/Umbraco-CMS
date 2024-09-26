import { UMB_SETTINGS_SECTION_PATHNAME } from '@umbraco-cms/backoffice/settings';
import { UMB_WORKSPACE_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';

export const UMB_RELATIONS_ROOT_WORKSPACE_PATH = UMB_WORKSPACE_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_SETTINGS_SECTION_PATHNAME,
	entityType: 'relations-root',
});
