import type { UmbWorkspaceViewNavigationContext } from './workspace-view-navigation.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT = new UmbContextToken<UmbWorkspaceViewNavigationContext>(
	'UmbWorkspaceViewNavigationContext',
);
