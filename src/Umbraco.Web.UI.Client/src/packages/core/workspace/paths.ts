import { UmbPathPattern } from '@umbraco-cms/backoffice/router';
import { UMB_SECTION_PATH_PATTERN } from '@umbraco-cms/backoffice/section';

export const UMB_WORKSPACE_PATH_PATTERN = new UmbPathPattern<
	{ entityType: string },
	typeof UMB_SECTION_PATH_PATTERN.ABSOLUTE_PARAMS
>('workspace/:entityType', UMB_SECTION_PATH_PATTERN);

export const UMB_WORKSPACE_EDIT_PATH_PATTERN = new UmbPathPattern<
	{ unique: string },
	typeof UMB_WORKSPACE_PATH_PATTERN.ABSOLUTE_PARAMS
>('edit/:unique', UMB_WORKSPACE_PATH_PATTERN);

export const UMB_WORKSPACE_EDIT_VARIANT_PATH_PATTERN = new UmbPathPattern<
	{ variantId: string },
	typeof UMB_WORKSPACE_EDIT_PATH_PATTERN.ABSOLUTE_PARAMS
>(':variantId', UMB_WORKSPACE_EDIT_PATH_PATTERN);

export const UMB_WORKSPACE_VIEW_PATH_PATTERN = new UmbPathPattern<
	{ viewPathname: string },
	typeof UMB_WORKSPACE_PATH_PATTERN.ABSOLUTE_PARAMS
>('view/:viewPathname', UMB_WORKSPACE_PATH_PATTERN);
