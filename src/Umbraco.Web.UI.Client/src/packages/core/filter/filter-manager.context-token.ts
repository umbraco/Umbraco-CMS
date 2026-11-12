import type { UmbFilterManagerContext } from './filter-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_FILTER_MANAGER_CONTEXT = new UmbContextToken<UmbFilterManagerContext>('UmbFilterManagerContext');
