import type { UmbWorkspaceContext } from '../workspace-context.interface.js';
import type { UmbNamableWorkspaceContext } from './namable-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { isObservable } from '@umbraco-cms/backoffice/external/rxjs';

export const UMB_NAMABLE_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext, UmbNamableWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbNamableWorkspaceContext =>
		(context as any).getName !== undefined && isObservable((context as any).name),
);
