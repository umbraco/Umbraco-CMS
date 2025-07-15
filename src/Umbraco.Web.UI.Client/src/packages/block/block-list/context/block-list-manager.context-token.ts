import type { UmbBlockListManagerContext } from './block-list-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_LIST_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockListManagerContext,
	UmbBlockListManagerContext
>('UmbBlockManagerContext');
