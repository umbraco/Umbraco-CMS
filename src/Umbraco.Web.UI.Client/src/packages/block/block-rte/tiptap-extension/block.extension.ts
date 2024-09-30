import { UMB_DATA_CONTENT_KEY, type UmbBlockRteLayoutModel } from '../types.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '../context/index.js';
import { UmbTiptapExtensionApiBase } from '@umbraco-cms/backoffice/tiptap';
import { Node } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';

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
			[UMB_DATA_CONTENT_KEY]: {
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
					const attrs = { [UMB_DATA_CONTENT_KEY]: options.contentUdi };
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
					const attrs = { [UMB_DATA_CONTENT_KEY]: options.contentUdi };
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},
});

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
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
	}

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}

	#updateBlocks(blocks: UmbBlockDataType[], layouts: Array<UmbBlockRteLayoutModel>) {
		const editor = this._editor;
		if (!editor) return;

		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_DATA_CONTENT_KEY),
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
