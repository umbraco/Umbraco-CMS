import type { UmbWorkspaceEditorNavigationContext } from './workspace-editor-navigation.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_WORKSPACE_EDITOR_NAVIGATION_CONTEXT = new UmbContextToken<UmbWorkspaceEditorNavigationContext>(
	'UmbWorkspaceViewNavigationContext',
);
