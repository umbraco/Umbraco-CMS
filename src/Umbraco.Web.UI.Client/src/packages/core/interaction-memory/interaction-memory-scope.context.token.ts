import type { UmbInteractionMemoryScopeContext } from './interaction-memory-scope.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_INTERACTION_MEMORY_SCOPE_CONTEXT = new UmbContextToken<UmbInteractionMemoryScopeContext>(
	'UmbInteractionMemoryScopeContext',
);
