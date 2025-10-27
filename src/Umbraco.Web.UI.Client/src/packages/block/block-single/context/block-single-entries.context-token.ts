import type { UmbBlockSingleEntriesContext } from './block-single-entries.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.17) [NL]
export const UMB_BLOCK_SINGLE_ENTRIES_CONTEXT = new UmbContextToken<UmbBlockSingleEntriesContext>(
	'UmbBlockEntriesContext',
);
