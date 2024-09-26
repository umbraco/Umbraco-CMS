import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockDataType } from '../../block/types.js';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

import '../components/block-rte-entry/index.js';

/**
 * A implementation of the Block Manager specifically for the Rich Text Editor.
 */
export class UmbBlockRteManagerContext<
	BlockLayoutType extends UmbBlockRteLayoutModel = UmbBlockRteLayoutModel,
> extends UmbBlockManagerContext<UmbBlockRteTypeModel, BlockLayoutType> {
	removeOneLayout(contentUdi: string) {
		this._layouts.removeOne(contentUdi);
	}

	create(contentElementTypeKey: string, partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>) {
		const data = super.createBlockData(contentElementTypeKey, partialLayoutEntry);

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

	insert(layoutEntry: BlockLayoutType, content: UmbBlockDataType, settings: UmbBlockDataType | undefined) {
		this._layouts.appendOne(layoutEntry);

		this.insertBlockData(layoutEntry, content, settings);

		return true;
	}

	/**
	 * @param contentUdi
	 * @internal
	 */
	public deleteLayoutElement(contentUdi: string) {
		this.removeBlockUdi(contentUdi);
	}
}
