import type { UmbBlockRteWorkspaceOriginData } from '../workspace/block-rte-workspace.modal-token.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

import '../components/block-rte-entry/index.js';

/**
 * A implementation of the Block Manager specifically for the Rich Text Editor.
 */
export class UmbBlockRteManagerContext<
	BlockLayoutType extends UmbBlockRteLayoutModel = UmbBlockRteLayoutModel,
> extends UmbBlockManagerContext<UmbBlockRteTypeModel, BlockLayoutType> {
	/**
	 * Pending deletions are used to support undo for block deletions.
	 * When a block is deleted via the delete button, the contentKey is added to this list.
	 * The Tiptap API observes this and removes the HTML element, which triggers
	 * the _filterUnusedBlocks mechanism that stores block data for undo and removes from manager.
	 */
	readonly #pendingDeletions = new UmbArrayState<string>([], (x) => x);
	public readonly pendingDeletions = this.#pendingDeletions.asObservable();

	/**
	 * Request a block to be deleted. This adds the contentKey to pending deletions,
	 * which will be processed by the Tiptap API to remove the HTML element first,
	 * enabling undo support.
	 * @param {string} contentKey - The content key of the block to delete.
	 */
	public requestPendingDeletion(contentKey: string) {
		this.#pendingDeletions.appendOne(contentKey);
	}

	/**
	 * Clear a pending deletion after it has been processed.
	 * @param {string} contentKey - The content key to clear from pending deletions.
	 */
	public clearPendingDeletion(contentKey: string) {
		this.#pendingDeletions.removeOne(contentKey);
	}

	removeOneLayout(contentKey: string) {
		this._layouts.removeOne(contentKey);
	}
	removeManyLayouts(contentKeys: Array<string>) {
		this._layouts.remove(contentKeys);
	}

	/**
	 * @param contentElementTypeKey
	 * @param partialLayoutEntry
	 * @param _originData
	 */
	async createWithPresets(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this, do not remove. [NL]

		_originData?: UmbBlockRteWorkspaceOriginData,
	) {
		const data = await super._createBlockData(contentElementTypeKey, partialLayoutEntry);

		// Find block type.
		const blockType = this.getBlockTypes().find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
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
		this.notifyBlockInserted(layoutEntry, originData);

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
