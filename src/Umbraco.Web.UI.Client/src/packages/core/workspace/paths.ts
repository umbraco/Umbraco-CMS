import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_WORKSPACE_PATH_PATTERN = new UmbPathPattern<
	{ entityType: string },
	typeof UMB_SECTION_PATH_PATTERN.ABSOLUTE_PARAMS
>('workspace/:entityType', UMB_SECTION_PATH_PATTERN);
