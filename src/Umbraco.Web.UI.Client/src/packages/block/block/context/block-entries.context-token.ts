import type { UmbBlockTypeBaseModel } from '../../block-type/types.js';
import type { UmbBlockLayoutBaseModel } from '../types.js';
import type { UmbBlockEntriesContext } from './block-entries.context.js';
import type { UMB_BLOCK_MANAGER_CONTEXT } from './block-manager.context-token.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

export const UMB_BLOCK_ENTRIES_CONTEXT = new UmbContextToken<
	UmbBlockEntriesContext<
		typeof UMB_BLOCK_MANAGER_CONTEXT,
		typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE,
		UmbBlockTypeBaseModel,
		UmbBlockLayoutBaseModel
	>
>('UmbBlockEntriesContext');
