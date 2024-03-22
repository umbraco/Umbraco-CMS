import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContextInterface } from './variant-dataset-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VARIANT_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbVariantDatasetWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbVariantDatasetWorkspaceContextInterface => 'variants' in context,
);
