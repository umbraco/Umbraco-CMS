import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import { UmbBlockManagerContext } from '../../block/context/block-manager.context.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import type { UmbBlockTypeGroup } from '../../block-type/types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext extends UmbBlockManagerContext<UmbBlockGridTypeModel, UmbBlockGridLayoutModel> {
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

	create(modalData: UmbBlockGridWorkspaceData, layoutEntry: UmbBlockGridLayoutModel, contentElementTypeKey: string) {
		return super.createBlock(modalData, layoutEntry, contentElementTypeKey, this.#createLayoutEntry);
	}

	#createLayoutEntry(
		modalData: UmbBlockGridWorkspaceData,
		layoutEntry: UmbBlockGridLayoutModel,
		contentElementTypeKey: string,
	) {
		// Here is room to append some extra layout properties if needed for this type.

		this._layouts.appendOneAt(layoutEntry, modalData.originData.index ?? -1);

		return true;
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockGridManagerContext,
	UmbBlockGridManagerContext
>('UmbBlockManagerContext');
