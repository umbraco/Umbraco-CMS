import type { UmbBlockGridEntryContext } from './block-grid-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_ENTRY_CONTEXT = new UmbContextToken<UmbBlockGridEntryContext>('UmbBlockEntryContext');
