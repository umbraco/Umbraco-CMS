import { UmbPathPattern } from '@umbraco-cms/backoffice/router';

export const UMB_SECTION_PATH_PATTERN = new UmbPathPattern<{ sectionName: string }>('section/:sectionName');
