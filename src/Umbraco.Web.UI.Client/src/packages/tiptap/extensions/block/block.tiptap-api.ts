import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteTypeModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	#blockTypes?: Map<string, UmbBlockRteTypeModel>;
	#managerContext?: typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			if (!context) return;

			this.#managerContext = context;

			this.observe(
				context.blockTypes,
				(blockTypes) => {
					this.#blockTypes = new Map(
						blockTypes.map((x) => [x.contentElementTypeKey, x] as [string, UmbBlockRteTypeModel]),
					);
				},
				'_observeBlockTypes',
			);

			this.observe(
				context.contents,
				(contents) => {
					this.#updateBlocks(contents);
				},
				'_observeContents',
			);

			this.observe(
				context.pendingDeletions,
				(pendingDeletions) => {
					this.#processPendingDeletions(pendingDeletions);
				},
				'_observePendingDeletions',
			);
		});
	}

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}

	/**
	 * Process pending deletions by removing blocks from the editor.
	 * This is triggered when blocks are deleted via the delete button.
	 * Removing HTML first enables undo support via _filterUnusedBlocks.
	 * @param {Array<string>} pendingDeletions - Array of content keys to delete.
	 */
	#processPendingDeletions(pendingDeletions: Array<string>) {
		if (pendingDeletions.length === 0) return;
		const editor = this._editor;
		if (!editor) return;

		// Remove blocks from editor
		this.#removeBlocksFromEditor(pendingDeletions);

		// Clear processed pending deletions
		pendingDeletions.forEach((contentKey) => {
			this.#managerContext?.clearPendingDeletion(contentKey);
		});
	}

	/**
	 * Sync blocks from manager contents to the editor.
	 * This adds blocks that exist in contents but not in the editor.
	 * Note: Removal is handled by _filterUnusedBlocks in the RTE base element,
	 * which enables undo support by storing block data before removal.
	 * @param {Array<UmbBlockDataModel>} contents - Array of block data models.
	 */
	#updateBlocks(contents: Array<UmbBlockDataModel>) {
		const editor = this._editor;
		if (!editor) return;

		if (!contents?.length) return;

		// Find existing blocks in the editor
		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY),
		);

		// ADD blocks that are in contents but NOT in editor
		const newBlocks = contents.filter((x) => !existingBlocks.includes(x.key));

		newBlocks.forEach((block) => {
			const inline = this.#blockTypes?.get(block.contentTypeKey)?.displayInline ?? false;
			if (inline) {
				editor.commands.setBlockInline({ contentKey: block.key });
			} else {
				editor.commands.setBlock({ contentKey: block.key });
			}
		});
	}

	/**
	 * Remove blocks from the editor by content keys.
	 * @param {Array<string>} contentKeys - Array of content keys to remove.
	 */
	#removeBlocksFromEditor(contentKeys: Array<string>) {
		const editor = this._editor;
		if (!editor) return;

		// Collect positions to delete
		const nodesToDelete: Array<{ pos: number; size: number }> = [];

		editor.state.doc.descendants((node, pos) => {
			const contentKey = node.attrs[UMB_BLOCK_RTE_DATA_CONTENT_KEY];
			if (contentKey && contentKeys.includes(contentKey)) {
				nodesToDelete.push({ pos, size: node.nodeSize });
			}
			return true; // Continue traversal to find all matches
		});

		if (nodesToDelete.length === 0) return;

		// Delete in reverse order (highest position first) to maintain valid positions
		nodesToDelete.sort((a, b) => b.pos - a.pos);

		let tr = editor.state.tr;
		for (const { pos, size } of nodesToDelete) {
			tr = tr.delete(pos, pos + size);
		}

		editor.view.dispatch(tr);
	}
}
