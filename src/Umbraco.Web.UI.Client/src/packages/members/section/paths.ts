import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_MEMBER_MANAGEMENT_SECTION_PATHNAME = 'member-management';

export const UMB_MEMBER_MANAGEMENT_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEMBER_MANAGEMENT_SECTION_PATHNAME,
});
