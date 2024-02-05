import type { UmbBlockGridContext } from './block-grid.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_CONTEXT = new UmbContextToken<UmbBlockGridContext>('UmbBlockContext');
