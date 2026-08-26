import type { UmbBlockRteWorkspaceOriginData } from '../workspace/block-rte-workspace.modal-token.js';
import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

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
	 * Creates block data with default presets for the given content element type.
	 * @param {string} contentElementTypeKey - The key of the content element type to create.
	 * @param {Omit<BlockLayoutType, 'contentKey'>} [partialLayoutEntry] - Partial layout entry to merge into the created layout entry.
	 * @param {UmbBlockRteWorkspaceOriginData} [_originData] - Origin data, unused by this implementation.
	 * @returns {Promise<{ layout: BlockLayoutType; content: UmbBlockDataModel; settings: UmbBlockDataModel | undefined }>} the created block data.
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
	 * @deprecated Use `removeOneContent` instead. Scheduled for removal in Umbraco 20.
	 * @param {string} contentKey - The content key of the layout element to delete.
	 * @internal
	 */
	public deleteLayoutElement(contentKey: string) {
		new UmbDeprecation({
			deprecated: 'deleteLayoutElement is deprecated.',
			removeInVersion: '20.0.0',
			solution: 'Use removeOneContent instead.',
		}).warn();
		this.removeOneContent(contentKey);
	}
}
