import type { UmbDictionaryWorkspaceContext } from './dictionary-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_DICTIONARY_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSubmittableWorkspaceContext,
	UmbDictionaryWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDictionaryWorkspaceContext => context.getEntityType?.() === 'dictionary',
);
