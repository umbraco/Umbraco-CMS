import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbEntityWorkspaceContextInterface } from './entity-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbEntityWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntityWorkspaceContextInterface => (context as any).getUnique !== undefined,
);
