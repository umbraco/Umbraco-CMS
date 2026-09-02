import type { UmbWorkspaceContext } from '../../workspace-context.interface.js';
import type { UmbEntryWithVariantsWorkspaceContext } from './entry-with-variants-workspace-context.interface.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_ENTRY_WITH_VARIANTS_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbEntryWithVariantsWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbEntryWithVariantsWorkspaceContext => (context as any).valueVariants !== undefined,
);
