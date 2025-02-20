import type { UmbLanguageAccessWorkspaceContext } from './language-access.workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_LANGUAGE_ACCESS_WORKSPACE_CONTEXT = new UmbContextToken<UmbLanguageAccessWorkspaceContext>(
	'UmbLanguageAccessWorkspaceContext',
);
