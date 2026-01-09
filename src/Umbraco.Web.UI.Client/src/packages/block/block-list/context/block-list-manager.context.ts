import type { UmbBlockListLayoutModel, UmbBlockListTypeModel } from '../types.js';
import type { UmbBlockListWorkspaceOriginData } from '../index.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UMB_PROPERTY_SORT_MODE_CONTEXT } from '@umbraco-cms/backoffice/property-sort-mode';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

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

	#sortModeContext?: typeof UMB_PROPERTY_SORT_MODE_CONTEXT.TYPE;
	#isSortMode = new UmbBooleanState(undefined);
	readonly isSortMode = this.#isSortMode.asObservable();

	setIsSortMode(isSortMode: boolean) {
		this.#sortModeContext?.setIsSortMode(isSortMode);
	}
	getIsSortMode(): boolean | undefined {
		return this.#sortModeContext?.getIsSortMode();
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_PROPERTY_SORT_MODE_CONTEXT, (sortPropertyContext) => {
			this.#sortModeContext = sortPropertyContext;
			this.observe(this.#sortModeContext?.isSortMode, (isSortMode) => {
				this.#isSortMode.setValue(isSortMode);
			});
		});
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
