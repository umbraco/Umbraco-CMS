import type { UmbBlockListLayoutModel, UmbBlockListTypeModel } from '../types.js';
import type { UmbBlockListWorkspaceOriginData } from '../index.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext, type UmbBlockDataObjectModel } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Block List Editor.
 */
export class UmbBlockListManagerContext<
	BlockLayoutType extends UmbBlockListLayoutModel = UmbBlockListLayoutModel,
> extends UmbBlockManagerContext<UmbBlockListTypeModel, BlockLayoutType, UmbBlockListWorkspaceOriginData> {
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
	 * @deprecated Use createWithPresets instead. Will be removed in v.17.
	 */
	create(
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		contentElementTypeKey: string,
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this. Do not remove. [NL]
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_originData?: UmbBlockListWorkspaceOriginData,
	): never {
		throw new Error('Method deparecated use createWithPresets');
	}
	async createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this. Do not remove. [NL]
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		_originData?: UmbBlockListWorkspaceOriginData,
	) {
		return await super._createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockListWorkspaceOriginData,
	) {
		this._layouts.appendOneAt(layoutEntry, originData.index ?? -1);

		this.insertBlockData(layoutEntry, content, settings, originData);

		return true;
	}
}
