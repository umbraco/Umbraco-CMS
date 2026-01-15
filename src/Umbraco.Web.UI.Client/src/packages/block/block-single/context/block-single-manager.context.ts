import type { UmbBlockSingleLayoutModel, UmbBlockSingleTypeModel } from '../types.js';
import type { UmbBlockSingleWorkspaceOriginData } from '../index.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Block Single Editor.
 */
export class UmbBlockSingleManagerContext<
	BlockLayoutType extends UmbBlockSingleLayoutModel = UmbBlockSingleLayoutModel,
> extends UmbBlockManagerContext<UmbBlockSingleTypeModel, BlockLayoutType, UmbBlockSingleWorkspaceOriginData> {
	//
	#inlineEditingMode = new UmbBooleanState(undefined);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();

	setInlineEditingMode(inlineEditingMode: boolean | undefined) {
		this.#inlineEditingMode.setValue(inlineEditingMode ?? false);
	}
	getInlineEditingMode(): boolean | undefined {
		return this.#inlineEditingMode.getValue();
	}

	/**
	 * @param contentElementTypeKey
	 * @param partialLayoutEntry
	 * @param _originData
	 */
	async createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this. Do not remove. [NL]

		_originData?: UmbBlockSingleWorkspaceOriginData,
	) {
		return await super._createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockSingleWorkspaceOriginData,
	) {
		this._layouts.appendOneAt(layoutEntry, originData.index ?? -1);

		this.insertBlockData(layoutEntry, content, settings, originData);

		return true;
	}
}
