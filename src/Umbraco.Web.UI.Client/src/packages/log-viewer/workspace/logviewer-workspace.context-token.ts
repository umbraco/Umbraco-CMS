import type { UmbLogViewerWorkspaceContext } from './logviewer-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_APP_LOG_VIEWER_CONTEXT = new UmbContextToken<UmbLogViewerWorkspaceContext>(
	'UmbLogViewerWorkspaceContext',
);
