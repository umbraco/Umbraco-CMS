import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_LIBRARY_SECTION_ALIAS = 'Umb.Section.Library';

export const UMB_LIBRARY_SECTION_PATHNAME = 'library';

export const UMB_LIBRARY_SECTION_PATH = UMB_SECTION_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_LIBRARY_SECTION_PATHNAME,
});
