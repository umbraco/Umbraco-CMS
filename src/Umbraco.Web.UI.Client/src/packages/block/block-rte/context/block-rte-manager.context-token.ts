import type { UmbBlockRteManagerContext } from './block-rte-manager.context.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

// TODO: Make discriminator method for this:
export const UMB_BLOCK_RTE_MANAGER_CONTEXT = new UmbContextToken<UmbBlockRteManagerContext, UmbBlockRteManagerContext>(
	'UmbBlockManagerContext',
);
