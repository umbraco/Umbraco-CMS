import type { UmbBlockEntryContext } from './block-entry.context.js';
import type { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_ENTRY_CONTEXT = new UmbContextToken<
	UmbBlockEntryContext<typeof UMB_BLOCK_MANAGER_CONTEXT, typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE>
>('UmbBlockEntryContext');
