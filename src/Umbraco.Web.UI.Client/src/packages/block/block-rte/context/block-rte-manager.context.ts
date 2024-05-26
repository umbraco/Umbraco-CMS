import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockRteWorkspaceData } from '../index.js';
import type { UmbBlockDataType } from '../../block/types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Rich Text Editor.
 */
export class UmbBlockRteManagerContext<
	BlockLayoutType extends UmbBlockRteLayoutModel = UmbBlockRteLayoutModel,
> extends UmbBlockManagerContext<UmbBlockRteTypeModel, BlockLayoutType> {
	//
	#inlineEditingMode = new UmbBooleanState(undefined);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();

	setInlineEditingMode(inlineEditingMode: boolean | undefined) {
		this.#inlineEditingMode.setValue(inlineEditingMode ?? false);
	}

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: UmbBlockRteWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockRteWorkspaceData,
	) {
		this._layouts.appendOneAt(layoutEntry, modalData.originData.index ?? -1);

		this.insertBlockData(layoutEntry, content, settings, modalData);

		return true;
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_RTE_MANAGER_CONTEXT = new UmbContextToken<UmbBlockRteManagerContext, UmbBlockRteManagerContext>(
	'UmbBlockManagerContext',
);
