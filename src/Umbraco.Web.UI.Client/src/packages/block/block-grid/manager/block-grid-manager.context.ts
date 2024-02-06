import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import { UmbBlockManagerContext } from '../../block/manager/block-manager.context.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import type { UmbBlockTypeGroup } from '../../block-type/types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext<
	BlockLayoutType extends UmbBlockGridLayoutModel = UmbBlockGridLayoutModel,
> extends UmbBlockManagerContext<UmbBlockGridTypeModel, BlockLayoutType> {
	//
	//
	#blockGroups = new UmbArrayState(<Array<UmbBlockTypeGroup>>[], (x) => x.key);
	public readonly blockGroups = this.#blockGroups.asObservable();

	setBlockGroups(blockGroups: Array<UmbBlockTypeGroup>) {
		this.#blockGroups.setValue(blockGroups);
	}
	getBlockGroups() {
		return this.#blockGroups.value;
	}

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
