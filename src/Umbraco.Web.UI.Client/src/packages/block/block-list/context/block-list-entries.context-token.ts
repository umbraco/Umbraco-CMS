import type { UmbBlockListEntriesContext } from './block-list-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.16) [NL]
export const UMB_BLOCK_LIST_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockListEntriesContext>('UmbBlockEntriesContext');
