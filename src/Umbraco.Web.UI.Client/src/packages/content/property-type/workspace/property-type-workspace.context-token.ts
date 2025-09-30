import type { UmbPropertyTypeWorkspaceContext } from './property-type-workspace.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

export const UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbPropertyTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPropertyTypeWorkspaceContext => (context as any).IS_PROPERTY_TYPE_WORKSPACE_CONTEXT,
);
