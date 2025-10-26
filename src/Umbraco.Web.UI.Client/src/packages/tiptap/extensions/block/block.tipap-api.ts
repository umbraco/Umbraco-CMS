import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { distinctUntilChanged } from '@umbraco-cms/backoffice/external/rxjs';
import { Node } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteLayoutModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbRteBlock: {
			setBlock: (options: { contentKey: string }) => ReturnType;
		};
		umbRteBlockInline: {
			setBlockInline: (options: { contentKey: string }) => ReturnType;
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
			[UMB_BLOCK_RTE_DATA_CONTENT_KEY]: {
				isRequired: true,
			},
		};
	},

	parseHTML() {
		return [{ tag: `umb-rte-block[${UMB_BLOCK_RTE_DATA_CONTENT_KEY}]` }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlock:
				(options) =>
				({ commands }) => {
					const attrs = { [UMB_BLOCK_RTE_DATA_CONTENT_KEY]: options.contentKey };
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},

	onPaste() {
		// Generate new contentKeys for pasted blocks to avoid duplication
		return (view: any, event: ClipboardEvent) => {
			const html = event.clipboardData?.getData('text/html');
			if (!html) return false;

			// Check if the pasted content contains blocks
			if (html.includes('umb-rte-block')) {
				// Replace contentKeys with new unique IDs
				const modifiedHtml = html.replace(
					/data-content-key="([^"]*)"/g,
					() => `data-content-key="${UmbId.new()}"`
				);

				// Insert the modified content
				const parser = new DOMParser();
				const doc = parser.parseFromString(modifiedHtml, 'text/html');
				const content = Array.from(doc.body.childNodes);

				view.dispatch(view.state.tr.replaceSelectionWith(
					view.state.schema.nodeFromDOM(doc.body).content
				));

				return true; // Prevent default paste behavior
			}
			return false;
		};
	},
});

const umbRteBlockInline = umbRteBlock.extend({
	name: 'umbRteBlockInline',
	group: 'inline',
	inline: true,

	parseHTML() {
		return [{ tag: `umb-rte-block-inline[${UMB_BLOCK_RTE_DATA_CONTENT_KEY}]` }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block-inline', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlockInline:
				(options) =>
				({ commands }) => {
					const attrs = { [UMB_BLOCK_RTE_DATA_CONTENT_KEY]: options.contentKey };
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},

	onPaste() {
		// Generate new contentKeys for pasted inline blocks to avoid duplication
		return (view: any, event: ClipboardEvent) => {
			const html = event.clipboardData?.getData('text/html');
			if (!html) return false;

			// Check if the pasted content contains inline blocks
			if (html.includes('umb-rte-block-inline')) {
				// Replace contentKeys with new unique IDs
				const modifiedHtml = html.replace(
					/data-content-key="([^"]*)"/g,
					() => `data-content-key="${UmbId.new()}"`
				);

				// Insert the modified content
				const parser = new DOMParser();
				const doc = parser.parseFromString(modifiedHtml, 'text/html');

				view.dispatch(view.state.tr.replaceSelectionWith(
					view.state.schema.nodeFromDOM(doc.body).content
				));

				return true; // Prevent default paste behavior
			}
			return false;
		};
	},
});

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
}
