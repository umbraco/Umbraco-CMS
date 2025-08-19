import type { UmbWorkspaceViewContext } from './workspace-view.context.js';
import type { UmbViewContext } from '@umbraco-cms/backoffice/view';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_WORKSPACE_VIEW_CONTEXT = new UmbContextToken<UmbViewContext, UmbWorkspaceViewContext>(
	'UmbViewContext',
	undefined,
	(context): context is UmbWorkspaceViewContext => (context as UmbWorkspaceViewContext).IS_WORKSPACE_VIEW_CONTEXT,
);
