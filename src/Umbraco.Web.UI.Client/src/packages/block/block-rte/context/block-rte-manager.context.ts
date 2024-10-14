import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

import '../components/block-rte-entry/index.js';

/**
 * A implementation of the Block Manager specifically for the Rich Text Editor.
 */
export class UmbBlockRteManagerContext<
	BlockLayoutType extends UmbBlockRteLayoutModel = UmbBlockRteLayoutModel,
> extends UmbBlockManagerContext<UmbBlockRteTypeModel, BlockLayoutType> {
	removeOneLayout(contentKey: string) {
		this._layouts.removeOne(contentKey);
	}

	create(contentElementTypeKey: string, partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>) {
		const data = super._createBlockData(contentElementTypeKey, partialLayoutEntry);

		// Find block type.
		const blockType = this.getBlockTypes().find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		if (blockType.displayInline) {
			data.layout.displayInline = true;
		}

		return data;
	}

	insert(layoutEntry: BlockLayoutType, content: UmbBlockDataModel, settings: UmbBlockDataModel | undefined) {
		this._layouts.appendOne(layoutEntry);

		this.insertBlockData(layoutEntry, content, settings);

		return true;
	}

	/**
	 * @param contentKey
	 * @internal
	 */
	public deleteLayoutElement(contentKey: string) {
		this.removeBlockKey(contentKey);
	}
}
