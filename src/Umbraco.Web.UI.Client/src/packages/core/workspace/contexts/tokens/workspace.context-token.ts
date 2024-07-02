import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from './workspace-context.interface.js';

export const UMB_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContext>('UmbWorkspaceContext');
