import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * A implementation of the Block Manager specifically for the Block List.
 */
export class UmbBlockListManagerContext extends UmbBlockManagerContext {
	createBlock(contentElementTypeKey: string) {
		return super._createBlockData({}, contentElementTypeKey);
	}
}

export const UMB_BLOCK_LIST_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockListManagerContext,
	UmbBlockListManagerContext
>('UmbBlockManagerContext');
