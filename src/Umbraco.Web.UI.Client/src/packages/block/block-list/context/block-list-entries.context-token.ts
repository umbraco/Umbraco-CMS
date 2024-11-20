import type { UmbBlockListEntriesContext } from './block-list-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_LIST_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockListEntriesContext>('UmbBlockEntriesContext');
