import type { UmbRelationTypeWorkspaceContext } from './relation-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbSaveableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_RELATION_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContext,
	UmbRelationTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbRelationTypeWorkspaceContext => context.getEntityType?.() === 'relation-type',
);
