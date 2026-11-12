import type { UmbFilterContext } from './filter.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FILTER_CONTEXT = new UmbContextToken<UmbFilterContext>('UmbFilterContext');
