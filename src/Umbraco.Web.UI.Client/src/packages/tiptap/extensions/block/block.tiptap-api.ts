import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_BLOCK_RTE_DATA_LAYOUT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	#managerContext?: typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			if (!context) return;

			this.#managerContext = context;

			this.observe(
				combineLatest([context.layouts, context.allContents]),
				([layouts]) => {
					this.#updateBlocks(layouts);
				},
				'_observeBlocks',
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
	 * Sync blocks from manager layouts to the editor.
	 * Observes both layouts (structure) and allContents (data availability) so that
	 * library element blocks are inserted once their external content has been fetched.
	 * Skips layout entries whose content type is not yet resolved (deferred until allContents re-emits).
	 * @param {Array<UmbBlockRteLayoutModel>} layouts - Current layout entries.
	 */
	#updateBlocks(layouts: Array<UmbBlockRteLayoutModel>) {
		const editor = this._editor;
		if (!editor) return;

		const existingLayoutKeys = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_BLOCK_RTE_DATA_LAYOUT_KEY),
		);

		const newLayouts = layouts.filter((x) => !existingLayoutKeys.includes(x.key));

		newLayouts.forEach((layout) => {
			const contentTypeKey = this.#managerContext?.getContentTypeKeyOfContentKey(layout.contentKey);
			if (!contentTypeKey) return; // External content not yet fetched — will retry when allContents emits.

			const inline = this.#managerContext?.getBlockTypeOf(contentTypeKey)?.displayInline ?? false;
			if (inline) {
				editor.commands.setBlockInline({ layoutKey: layout.key, contentKey: layout.contentKey });
			} else {
				editor.commands.setBlock({ layoutKey: layout.key, contentKey: layout.contentKey });
			}
		});
	}

	/**
	 * Process pending deletions by removing blocks from the editor.
	 * This is triggered when blocks are deleted via the delete button.
	 * Removing HTML first enables undo support via _filterUnusedBlocks.
	 * @param {Array<string>} pendingDeletions - Array of layout keys to delete.
	 */
	#processPendingDeletions(pendingDeletions: Array<string>) {
		if (pendingDeletions.length === 0) return;
		const editor = this._editor;
		if (!editor) return;

		this.#removeBlocksFromEditor(pendingDeletions);

		pendingDeletions.forEach((layoutKey) => {
			this.#managerContext?.clearPendingDeletion(layoutKey);
		});
	}

	/**
	 * Remove blocks from the editor by layout keys.
	 * @param {Array<string>} layoutKeys - Array of layout keys to remove.
	 */
	#removeBlocksFromEditor(layoutKeys: Array<string>) {
		const editor = this._editor;
		if (!editor) return;

		const nodesToDelete: Array<{ pos: number; size: number }> = [];

		editor.state.doc.descendants((node, pos) => {
			const layoutKey = node.attrs[UMB_BLOCK_RTE_DATA_LAYOUT_KEY];
			if (layoutKey && layoutKeys.includes(layoutKey)) {
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
