import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceViewContext } from './workspace-view.context.js';

export const UMB_WORKSPACE_VIEW_CONTEXT = new UmbContextToken<UmbWorkspaceViewContext>('UmbWorkspaceViewContext');
