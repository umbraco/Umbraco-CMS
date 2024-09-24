import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '../context/block-rte-entries.context-token.js';
import type { UmbBlockDataType } from '../../block/types.js';
import { UMB_DATA_CONTENT_UDI, type UmbBlockRteLayoutModel } from '../types.js';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbTiptapToolbarElementApiBase } from '@umbraco-cms/backoffice/tiptap';
import { Node, type Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbRteBlock: {
			setBlock: (options: { contentUdi: string }) => ReturnType;
		};
		umbRteBlockInline: {
			setBlockInline: (options: { contentUdi: string }) => ReturnType;
		};
	}
}

const umbRteBlock = Node.create({
	name: 'umbRteBlock',
	group: 'block',
	content: undefined, // The block does not have any content, it is just a wrapper.
	atom: true, // The block is an atom, meaning it is a single unit that cannot be split.
	marks: '', // We do not allow marks on the block
	draggable: true,
	selectable: true,

	addAttributes() {
		return {
			[UMB_DATA_CONTENT_UDI]: {
				isRequired: true,
			},
		};
	},

	parseHTML() {
		return [{ tag: 'umb-rte-block' }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlock:
				(options) =>
				({ commands }) => {
					const attrs = { [UMB_DATA_CONTENT_UDI]: options.contentUdi };
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},
});

const umbRteBlockInline = umbRteBlock.extend({
	name: 'umbRteBlockInline',
	group: 'inline',
	inline: true,

	parseHTML() {
		return [{ tag: 'umb-rte-block-inline' }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block-inline', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlockInline:
				(options) =>
				({ commands }) => {
					const attrs = { [UMB_DATA_CONTENT_UDI]: options.contentUdi };
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},
});

export default class UmbTiptapBlockPickerExtension extends UmbTiptapToolbarElementApiBase {
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

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}

	override isActive(editor: Editor) {
		return (
			editor.isActive(`umb-rte-block[${UMB_DATA_CONTENT_UDI}]`) ||
			editor.isActive(`umb-rte-block-inline[${UMB_DATA_CONTENT_UDI}]`)
		);
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
