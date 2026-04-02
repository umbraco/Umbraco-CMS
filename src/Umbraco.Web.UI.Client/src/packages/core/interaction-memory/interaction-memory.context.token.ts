import type { UmbInteractionMemoryContext } from './interaction-memory.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_INTERACTION_MEMORY_CONTEXT = new UmbContextToken<UmbInteractionMemoryContext>(
	'UmbInteractionMemoryContext',
);
