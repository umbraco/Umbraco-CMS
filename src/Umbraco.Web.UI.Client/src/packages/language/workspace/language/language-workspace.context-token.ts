import type { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSaveableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_LANGUAGE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContext,
	UmbLanguageWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbLanguageWorkspaceContext => context.getEntityType?.() === 'language',
);
