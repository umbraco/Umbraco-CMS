import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbVariantDatasetWorkspaceContext } from './variant-dataset-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VARIANT_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbVariantDatasetWorkspaceContext
>('UmbWorkspaceContext', undefined, (context): context is UmbVariantDatasetWorkspaceContext => 'variants' in context);
