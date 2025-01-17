import type { UmbLanguageWorkspaceContext } from './language-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_LANGUAGE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbLanguageWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbLanguageWorkspaceContext => context.getEntityType?.() === 'language',
);
