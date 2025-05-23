import { UMB_DASHBOARD_PATH_PATTERN } from '@umbraco-cms/backoffice/dashboard';
import { UMB_TRANSLATION_SECTION_PATHNAME } from '@umbraco-cms/backoffice/translation';

export const UMB_DICTIONARY_OVERVIEW_DASHBOARD_PATHNAME = 'dictionary-overview';

export const UMB_DICTIONARY_OVERVIEW_DASHBOARD_PATH = UMB_DASHBOARD_PATH_PATTERN.generateAbsolute({
	sectionName: UMB_TRANSLATION_SECTION_PATHNAME,
	dashboardPathname: UMB_DICTIONARY_OVERVIEW_DASHBOARD_PATHNAME,
});
