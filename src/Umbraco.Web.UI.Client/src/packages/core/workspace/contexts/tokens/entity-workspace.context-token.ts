import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbEntityWorkspaceContext } from './entity-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTITY_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbEntityWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntityWorkspaceContext => (context as any).getUnique !== undefined,
);
