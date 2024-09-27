import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '../context/block-rte-entries.context-token.js';
import type { UmbBlockDataType } from '../../block/types.js';
import { UMB_DATA_CONTENT_UDI, type UmbBlockRteLayoutModel } from '../types.js';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbTiptapToolbarElementApiBase } from '@umbraco-cms/backoffice/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';

export default class UmbTiptapBlockPickerToolbarExtension extends UmbTiptapToolbarElementApiBase {
	#blocks?: Array<UmbBlockTypeBaseModel>;
	#entriesContext?: typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			this.observe(
				context.blockTypes,
				(blockTypes) => {
					this.#blocks = blockTypes;
				},
				'blockType',
			);
			this.observe(
				context.contents.pipe(
					distinctUntilChanged((prev, curr) => prev.map((y) => y.udi).join() === curr.map((y) => y.udi).join()),
				),
				(contents) => {
					this.#updateBlocks(contents, context.getLayouts());
				},
				'contents',
			);
		});
		this.consumeContext(UMB_BLOCK_RTE_ENTRIES_CONTEXT, (context) => {
			this.#entriesContext = context;
		});
	}

	override isActive(editor: Editor) {
		return editor.isActive('umbRteBlock') || editor.isActive('umbRteBlockInline');
	}

	override async execute() {
		return this.#createBlock();
	}

	#createBlock() {
		if (!this.#entriesContext) {
			console.error('[Block Picker] No entries context available.');
			return;
		}

		let createPath: string | undefined = undefined;

		if (this.#blocks?.length === 1) {
			const elementKey = this.#blocks[0].contentElementTypeKey;
			createPath = this.#entriesContext.getPathForCreateBlock() + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this.#entriesContext.getPathForCreateBlock();
		}

		if (createPath) {
			window.history.pushState({}, '', createPath);
		}
	}

	#updateBlocks(blocks: UmbBlockDataType[], layouts: Array<UmbBlockRteLayoutModel>) {
		const editor = this._editor;
		if (!editor) return;

		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_DATA_CONTENT_UDI),
		);
		const newBlocks = blocks.filter((x) => !existingBlocks.find((contentUdi) => contentUdi === x.udi));

		newBlocks.forEach((block) => {
			// Find layout for block
			const layout = layouts.find((x) => x.contentUdi === block.udi);
			const inline = layout?.displayInline ?? false;

			if (inline) {
				editor.commands.setBlockInline({ contentUdi: block.udi });
			} else {
				editor.commands.setBlock({ contentUdi: block.udi });
			}
		});
	}
}
