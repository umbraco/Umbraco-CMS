import type { UmbBlockGridManagerContext } from './block-grid-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this: (Aim to do this for v.16) [NL]
export const UMB_BLOCK_GRID_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockGridManagerContext,
	UmbBlockGridManagerContext
>('UmbBlockManagerContext');
