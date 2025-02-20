import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_USER_SECTION_PATHNAME = 'user-management';

export const UMB_USER_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_USER_SECTION_PATHNAME,
});
