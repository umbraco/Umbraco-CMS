import type { UmbBlockListLayoutModel, UmbBlockListTypeModel } from '../types.js';
import type { UmbBlockListWorkspaceData } from '../index.js';
import type { UmbBlockDataType } from '../../block/types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

/**
 * A implementation of the Block Manager specifically for the Block List Editor.
 */
export class UmbBlockListManagerContext<
	BlockLayoutType extends UmbBlockListLayoutModel = UmbBlockListLayoutModel,
> extends UmbBlockManagerContext<UmbBlockListTypeModel, BlockLayoutType> {
	//
	#inlineEditingMode = new UmbBooleanState(undefined);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();

	setInlineEditingMode(inlineEditingMode: boolean | undefined) {
		this.#inlineEditingMode.setValue(inlineEditingMode ?? false);
	}

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: UmbBlockListWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockListWorkspaceData,
	) {
		this._layouts.appendOneAt(layoutEntry, modalData.originData.index ?? -1);

		this.insertBlockData(layoutEntry, content, settings, modalData);

		return true;
	}

	updateLayout(layoutEntry: Partial<BlockLayoutType> & Pick<BlockLayoutType, 'contentUdi'>): boolean {
		this._layouts.updateOne(layoutEntry.contentUdi, layoutEntry);
		return true; // Should have returned false if it did not find the one to update.. but in this case we just insert it if it does not exist. [NL]
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_LIST_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockListManagerContext,
	UmbBlockListManagerContext
>('UmbBlockManagerContext');
