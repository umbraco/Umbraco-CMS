import type { UmbWorkspaceContextInterface } from './workspace-context.interface.js';
import type { UmbVariantableWorkspaceContextInterface } from './workspace-variantable-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_VARIANT_WORKSPACE_CONTEXT_TOKEN = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbVariantableWorkspaceContextInterface
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbVariantableWorkspaceContextInterface => 'variants' in context,
);
