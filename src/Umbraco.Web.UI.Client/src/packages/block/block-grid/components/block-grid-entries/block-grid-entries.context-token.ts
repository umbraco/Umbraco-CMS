import type { UmbBlockGridEntriesContext } from './block-grid-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockGridEntriesContext>('UmbBlockEntriesContext');
