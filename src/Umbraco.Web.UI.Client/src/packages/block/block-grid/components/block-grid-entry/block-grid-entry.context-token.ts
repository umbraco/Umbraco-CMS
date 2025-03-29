import type { UmbBlockGridEntryContext } from './block-grid-entry.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.16) [NL]
export const UMB_BLOCK_GRID_ENTRY_CONTEXT = new UmbContextToken<UmbBlockGridEntryContext>('UmbBlockEntryContext');
