import type { UmbBlockRteWorkspaceOriginData } from '../workspace/block-rte-workspace.modal-token.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbBlockManagerContext, type UmbBlockDataObjectModel } from '@umbraco-cms/backoffice/block';

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
	removeManyLayouts(contentKeys: Array<string>) {
		this._layouts.remove(contentKeys);
	}

	/**
	 * @deprecated Use createWithPresets instead. Will be removed in v.17.
	 */
	create(
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		contentElementTypeKey: string,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this. Do not remove. [NL]
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_originData?: UmbBlockRteWorkspaceOriginData,
	) {
		throw new Error('Method deparecated use createWithPresets');
		return {} as UmbBlockDataObjectModel<BlockLayoutType>;
	}
	async createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this, do not remove. [NL]
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_originData?: UmbBlockRteWorkspaceOriginData,
	) {
		const data = await super._createBlockData(contentElementTypeKey, partialLayoutEntry);

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

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockRteWorkspaceOriginData,
	) {
		this._layouts.appendOne(layoutEntry);

		this.insertBlockData(layoutEntry, content, settings, originData);

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
