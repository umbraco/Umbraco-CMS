import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_WORKSPACE_PATH_PATTERN = new UmbPathPattern<
	{ workspaceAlias: string },
	typeof UMB_SECTION_PATH_PATTERN.ABSOLUTE_PARAMS
>('/workspace/:workspaceAlias', UMB_SECTION_PATH_PATTERN);
