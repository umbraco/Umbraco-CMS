import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbPropertyStructureWorkspaceContext } from './property-structure-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbPropertyStructureWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPropertyStructureWorkspaceContext => 'propertyStructureById' in context,
);
