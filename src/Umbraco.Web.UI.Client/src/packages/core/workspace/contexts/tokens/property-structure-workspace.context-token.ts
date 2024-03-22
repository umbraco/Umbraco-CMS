import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbPropertyStructureWorkspaceContextInterface } from './property-structure-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbPropertyStructureWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbPropertyStructureWorkspaceContextInterface => 'propertyStructureById' in context,
);
