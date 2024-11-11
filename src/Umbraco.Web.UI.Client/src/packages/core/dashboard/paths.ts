import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';
import { UmbPathPattern } from '@umbraco-cms/backoffice/router';

export const UMB_DASHBOARD_PATH_PATTERN = new UmbPathPattern<
	{ dashboardPathname: string },
	typeof UMB_SECTION_PATH_PATTERN.ABSOLUTE_PARAMS
>('dashboard/:dashboardPathname', UMB_SECTION_PATH_PATTERN);
