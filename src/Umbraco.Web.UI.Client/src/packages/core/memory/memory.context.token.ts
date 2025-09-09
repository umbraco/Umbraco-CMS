import type { UmbMemoryContext } from './memory.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_MEMORY_CONTEXT = new UmbContextToken<UmbMemoryContext>('UmbMemoryContext');
