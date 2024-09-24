import type { UmbBlockDataType } from '../../block/types.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '../context/block-rte-entries.context-token.js';
import { UMB_DATA_CONTENT_UDI, type UmbBlockRteLayoutModel } from '../types.js';
import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/tiny-mce';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';

export default class UmbTinyMceMultiUrlPickerPlugin extends UmbTinyMcePluginBase {
	#localize = new UmbLocalizationController(this._host);
	#editor: Editor;
	#blocks?: Array<UmbBlockTypeBaseModel>;
	#entriesContext?: typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.#editor = args.editor;

		args.editor.ui.registry.addToggleButton('umbblockpicker', {
			icon: 'visualblocks',
			tooltip: this.#localize.term('blockEditor_insertBlock'),
			onAction: () => this.showDialog(),
			onSetup: function (api) {
				const changed = args.editor.selection.selectorChangedWithUnbind(
					'umb-rte-block[data-content-udi], umb-rte-block-inline[data-content-udi]',
					(state) => api.setActive(state),
				);
				return () => changed.unbind();
			},
		});

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			this.observe(
				context.blockTypes,
				(blockTypes) => {
					this.#blocks = blockTypes;
				},
				'blockType',
			);

			this.observe(
				context.contents,
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

	async showDialog() {
		//const blockEl = this.editor.selection.getNode();

		/*if (blockEl.nodeName === 'UMB-RTE-BLOCK' || blockEl.nodeName === 'UMB-RTE-BLOCK-INLINE') {
			const blockUdi = blockEl.getAttribute('data-content-udi') ?? undefined;
			if (blockUdi) {
				// TODO: Missing a solution to edit a block from this scope. [NL]
				this.#editBlock(blockUdi);
				return;
			}
		}*/

		// If no block is selected, open the block picker:
		this.#createBlock();
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
		const editor = this.#editor;
		if (!editor?.dom) return;

		const existingBlocks = editor.dom
			.select('umb-rte-block, umb-rte-block-inline')
			.map((x) => x.getAttribute(UMB_DATA_CONTENT_UDI));
		const newBlocks = blocks.filter((x) => !existingBlocks.find((contentUdi) => contentUdi === x.udi));

		newBlocks.forEach((block) => {
			// Find layout for block
			const layout = layouts.find((x) => x.contentUdi === block.udi);
			const inline = layout?.displayInline ?? false;

			let blockTag = 'umb-rte-block';

			if (inline) {
				blockTag = 'umb-rte-block-inline';
			}

			editor.insertContent(`<${blockTag} data-content-udi="${block.udi}"></${blockTag}>`);
		});
	}
}
