import type { UmbBlockSingleManagerContext } from './block-single-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.16) [NL]
export const UMB_BLOCK_SINGLE_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockSingleManagerContext,
	UmbBlockSingleManagerContext
>('UmbBlockManagerContext');
