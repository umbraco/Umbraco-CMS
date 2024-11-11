import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbNamableWorkspaceContext } from './namable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_NAMABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbNamableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbNamableWorkspaceContext => (context as any).getName !== undefined,
);
