import { Node } from '../../externals.js';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY, UMB_BLOCK_RTE_DATA_LAYOUT_KEY } from '@umbraco-cms/backoffice/rte';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbRteBlock: {
			setBlock: (options: { layoutKey: string; contentKey: string }) => ReturnType;
		};
		umbRteBlockInline: {
			setBlockInline: (options: { layoutKey: string; contentKey: string }) => ReturnType;
		};
	}
}

export const umbRteBlock = Node.create({
	name: 'umbRteBlock',
	group: 'block',
	content: undefined, // The block does not have any content, it is just a wrapper.
	atom: true, // The block is an atom, meaning it is a single unit that cannot be split.
	marks: '', // We do not allow marks on the block
	draggable: true,
	selectable: true,

	addAttributes() {
		return {
			[UMB_BLOCK_RTE_DATA_LAYOUT_KEY]: {
				isRequired: true,
			},
			[UMB_BLOCK_RTE_DATA_CONTENT_KEY]: {
				isRequired: true,
			},
		};
	},

	parseHTML() {
		return [
			{
				tag: `umb-rte-block[${UMB_BLOCK_RTE_DATA_CONTENT_KEY}]`,
				getAttrs: (node) => {
					const el = node as HTMLElement;
					const contentKey = el.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY);
					// Migrate legacy markup that has no data-key: use content key as layout key.
					// Safe because setLayouts() coerces layout.key ??= layout.contentKey for persisted data.
					const layoutKey = el.getAttribute(UMB_BLOCK_RTE_DATA_LAYOUT_KEY) ?? contentKey;
					return { [UMB_BLOCK_RTE_DATA_LAYOUT_KEY]: layoutKey, [UMB_BLOCK_RTE_DATA_CONTENT_KEY]: contentKey };
				},
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlock:
				(options) =>
				({ commands }) => {
					const attrs = {
						[UMB_BLOCK_RTE_DATA_LAYOUT_KEY]: options.layoutKey,
						[UMB_BLOCK_RTE_DATA_CONTENT_KEY]: options.contentKey,
					};
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},
});

export const umbRteBlockInline = umbRteBlock.extend({
	name: 'umbRteBlockInline',
	group: 'inline',
	inline: true,

	parseHTML() {
		return [
			{
				tag: `umb-rte-block-inline[${UMB_BLOCK_RTE_DATA_CONTENT_KEY}]`,
				getAttrs: (node) => {
					const el = node as HTMLElement;
					const contentKey = el.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY);
					const layoutKey = el.getAttribute(UMB_BLOCK_RTE_DATA_LAYOUT_KEY) ?? contentKey;
					return { [UMB_BLOCK_RTE_DATA_LAYOUT_KEY]: layoutKey, [UMB_BLOCK_RTE_DATA_CONTENT_KEY]: contentKey };
				},
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return ['umb-rte-block-inline', HTMLAttributes];
	},

	addCommands() {
		return {
			setBlockInline:
				(options) =>
				({ commands }) => {
					const attrs = {
						[UMB_BLOCK_RTE_DATA_LAYOUT_KEY]: options.layoutKey,
						[UMB_BLOCK_RTE_DATA_CONTENT_KEY]: options.contentKey,
					};
					return commands.insertContent({
						type: this.name,
						attrs,
					});
				},
		};
	},
});
