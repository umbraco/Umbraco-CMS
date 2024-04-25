import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_SETTINGS_SECTION_PATHNAME = 'settings';
export const UMB_SETTINGS_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	name: UMB_SETTINGS_SECTION_PATHNAME,
});
