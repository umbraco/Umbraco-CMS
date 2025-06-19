import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_TRANSLATION_SECTION_PATHNAME = 'translation';
export const UMB_TRANSLATION_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_TRANSLATION_SECTION_PATHNAME,
});
