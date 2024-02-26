import type { UmbBlockListEntryContext } from './block-list-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_LIST_ENTRY_CONTEXT = new UmbContextToken<UmbBlockListEntryContext>('UmbBlockEntryContext');
