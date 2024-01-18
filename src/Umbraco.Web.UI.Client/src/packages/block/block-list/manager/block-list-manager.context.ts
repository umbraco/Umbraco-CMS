import type { UmbBlockListLayoutModel, UmbBlockListTypeModel } from '../types.js';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * A implementation of the Block Manager specifically for the Block List.
 */
export class UmbBlockListManagerContext<
	BlockLayoutType extends UmbBlockListLayoutModel = UmbBlockListLayoutModel,
> extends UmbBlockManagerContext<UmbBlockListTypeModel, BlockLayoutType> {
	createBlock(layoutEntry: Omit<BlockLayoutType, 'contentUdi'>, contentElementTypeKey: string) {
		// Here is room to append some extra layout properties if needed for this type.

		return super._createBlockData(layoutEntry, contentElementTypeKey);
	}
}

export const UMB_BLOCK_LIST_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockListManagerContext,
	UmbBlockListManagerContext
>('UmbBlockManagerContext');
