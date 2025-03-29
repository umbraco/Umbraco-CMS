import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceViewNavigationContext } from './workspace-view-navigation.context';

export const UMB_WORKSPACE_VIEW_NAVIGATION_CONTEXT = new UmbContextToken<
	UmbWorkspaceViewNavigationContext,
	UmbWorkspaceViewNavigationContext
>('UmbWworkspaceViewNavigationCcontext');
