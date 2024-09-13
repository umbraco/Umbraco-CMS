import type { UmbBlockLayoutBaseModel } from '../types.js';
import type { UmbBlockWorkspaceOriginData } from '../workspace/block-workspace.modal-token.js';
import type { UMB_BLOCK_ENTRIES_CONTEXT } from './block-entries.context-token.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UmbBlockEntryContext } from './block-entry.context.js';
import type { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_ENTRY_CONTEXT = new UmbContextToken<
	UmbBlockEntryContext<
		typeof UMB_BLOCK_MANAGER_CONTEXT,
		typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE,
		typeof UMB_BLOCK_ENTRIES_CONTEXT,
		UmbBlockEntriesContext<
			typeof UMB_BLOCK_MANAGER_CONTEXT,
			typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE,
			UmbBlockTypeBaseModel,
			UmbBlockLayoutBaseModel,
			UmbBlockWorkspaceOriginData
		>,
		UmbBlockTypeBaseModel,
		UmbBlockLayoutBaseModel
	>
>('UmbBlockEntryContext');
