import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_DOCUMENTS_SECTION_PATHNAME = 'content';
export const UMB_DOCUMENTS_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_DOCUMENTS_SECTION_PATHNAME,
});
