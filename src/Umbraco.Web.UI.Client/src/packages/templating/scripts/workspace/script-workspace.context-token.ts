import type { UmbScriptWorkspaceContext } from './script-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_SCRIPT_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbScriptWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbScriptWorkspaceContext => context.getEntityType?.() === 'script',
);
