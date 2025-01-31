import type { UmbBlockRteEntryContext } from './block-rte-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_RTE_ENTRY_CONTEXT = new UmbContextToken<UmbBlockRteEntryContext>('UmbBlockEntryContext');
