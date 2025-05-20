import type { UmbBlockRteEntriesContext } from './block-rte-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.16) [NL]
export const UMB_BLOCK_RTE_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockRteEntriesContext>('UmbBlockEntriesContext');
