import type { UmbBlockRteEntriesContext } from './block-rte-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_RTE_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockRteEntriesContext>('UmbBlockEntriesContext');
