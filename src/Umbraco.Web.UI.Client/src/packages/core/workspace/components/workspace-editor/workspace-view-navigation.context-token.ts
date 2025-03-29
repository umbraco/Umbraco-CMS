import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceViewNavigationContext } from './workspace-view-navigation.context.js';

export const UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT = new UmbContextToken<UmbWorkspaceViewNavigationContext>(
	'UmbWorkspaceViewNavigationContext',
);
