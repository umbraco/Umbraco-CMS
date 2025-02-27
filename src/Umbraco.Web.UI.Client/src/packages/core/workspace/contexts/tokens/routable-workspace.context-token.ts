import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbRoutableWorkspaceContext } from './routable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ROUTABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbRoutableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbRoutableWorkspaceContext => 'routes' in context,
);
