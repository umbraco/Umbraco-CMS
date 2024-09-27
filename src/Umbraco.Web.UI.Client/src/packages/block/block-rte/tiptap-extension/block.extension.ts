import { UMB_DATA_CONTENT_UDI } from '../types.js';
import { UmbTiptapExtensionApiBase } from '@umbraco-cms/backoffice/tiptap';
import { Node } from '@umbraco-cms/backoffice/external/tiptap';

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

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}
}
