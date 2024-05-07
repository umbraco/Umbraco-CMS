import { UmbPathPattern } from '@umbraco-cms/backoffice/router';

export const UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN = new UmbPathPattern<{ unique: string }>('edit/:unique');
