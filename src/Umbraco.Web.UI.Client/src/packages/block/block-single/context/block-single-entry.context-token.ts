import type { UmbBlockSingleEntryContext } from './block-single-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_SINGLE_ENTRY_CONTEXT = new UmbContextToken<UmbBlockSingleEntryContext>('UmbBlockEntryContext');
