import type { UmbRelationTypeWorkspaceContext } from './relation-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make readonly workspace context type
export const UMB_RELATION_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<any, UmbRelationTypeWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbRelationTypeWorkspaceContext => context.getEntityType?.() === 'relation-type',
);
