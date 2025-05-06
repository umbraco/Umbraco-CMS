import type { UmbWorkspaceViewContext } from './workspace-view.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_WORKSPACE_VIEW_CONTEXT = new UmbContextToken<UmbWorkspaceViewContext>('UmbWorkspaceViewContext');
