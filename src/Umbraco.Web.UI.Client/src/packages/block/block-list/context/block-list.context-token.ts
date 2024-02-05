import type { UmbBlockListContext } from './block-list.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_LIST_CONTEXT = new UmbContextToken<UmbBlockListContext>('UmbBlockContext');
