import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import { UmbBlockManagerContext } from '../../block/manager/block-manager.context.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext<
	BlockLayoutType extends UmbBlockGridLayoutModel = UmbBlockGridLayoutModel,
> extends UmbBlockManagerContext<UmbBlockGridTypeModel, BlockLayoutType> {
	//
	/*
	#inlineEditingMode = new UmbBooleanState(undefined);
	inlineEditingMode = this.#inlineEditingMode.asObservable();

	setInlineEditingMode(inlineEditingMode: boolean | undefined) {
		this.#inlineEditingMode.setValue(inlineEditingMode ?? false);
	}
	*/

	_createBlock(modalData: UmbBlockGridWorkspaceData, layoutEntry: BlockLayoutType, contentElementTypeKey: string) {
		// Here is room to append some extra layout properties if needed for this type.

		this._layouts.appendOneAt(layoutEntry, modalData.originData.index ?? -1);

		// TODO: Ability to add at a specific Area.

		return true;
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockGridManagerContext,
	UmbBlockGridManagerContext
>('UmbBlockManagerContext');
