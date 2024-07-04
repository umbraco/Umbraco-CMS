import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_MEDIA_SECTION_PATHNAME = 'media';
export const UMB_MEDIA_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_MEDIA_SECTION_PATHNAME,
});
