import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import type { Editor } from '@tiptap/core';
// eslint-disable-next-line local-rules/enforce-umbraco-external-imports
import type { Slice } from '@tiptap/pm/model';

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			this.observe(
				context?.contents.pipe(
					distinctUntilChanged((prev, curr) => prev.map((y) => y.key).join() === curr.map((y) => y.key).join()),
				),
				(contents) => {
					if (!contents || !context) {
						return;
					}
					this.#updateBlocks(contents, context.getLayouts());
				},
				'contents',
			);
		});
	}

	#handlePaste = ({ editor, event, slice }: { editor: Editor; event: ClipboardEvent; slice: Slice }) => {
		const html = event.clipboardData?.getData('text/html');
		if (!html) {
			return;
		}

		console.log('umbRteBlockInlinePasteHandler.handlePaste', [editor, event, slice, html]);

		// Check if the HTML contains an umb-rte-block-inline element
		// If it does, then loop over the elements and insert them as inline blocks
		// For each copied block, call the block RTE manager context to clone the block properties
		// Give the pasted block a new content key
	};

	override setEditor(editor: Editor): void {
		super.setEditor(editor);
		editor.on('paste', this.#handlePaste);
	}

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}

	#updateBlocks(blocks: UmbBlockDataModel[], layouts: Array<UmbBlockRteLayoutModel>) {
		const editor = this._editor;
		if (!editor) return;

		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY),
		);
		const newBlocks = blocks.filter((x) => !existingBlocks.find((contentKey) => contentKey === x.key));

		newBlocks.forEach((block) => {
			// Find layout for block
			const layout = layouts.find((x) => x.contentKey === block.key);
			const inline = layout?.displayInline ?? false;

			if (inline) {
				editor.commands.setBlockInline({ contentKey: block.key });
			} else {
				editor.commands.setBlock({ contentKey: block.key });
			}
		});
	}

	override destroy(): void {
		super.destroy();
		this._editor?.off('paste', this.#handlePaste);
	}
}
