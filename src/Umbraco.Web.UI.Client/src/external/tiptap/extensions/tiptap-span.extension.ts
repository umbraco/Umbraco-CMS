import { Mark, mergeAttributes } from '@tiptap/core';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export interface SpanOptions {
	/**
	 * HTML attributes to add to the span element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

function parseStyles(style: string | undefined): Record<string, string> {
	const items: Record<string, string> = {};

	(style ?? '')
		.split(';')
		.map((x) => x.trim())
		.filter((x) => x)
		.forEach((rule) => {
			const [key, value] = rule.split(':');
			if (key && value) {
				items[key.trim()] = value.trim();
			}
		});

	return items;
}

function serializeStyles(items: Record<string, string>): string {
	return (
		Object.entries(items)
			.map(([key, value]) => `${key}: ${value}`)
			.join(';') + ';'
	);
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const Span = Mark.create<SpanOptions>({
	name: 'span',

	priority: 50,

	addOptions() {
		return { HTMLAttributes: {} };
	},

	parseHTML() {
		return [{ tag: this.name }];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},

	addCommands() {
		return {
			setSpanStyle:
				(styles) =>
				({ commands, editor }) => {
					if (!styles) return false;

					const existing = editor.getAttributes(this.name)?.style as string;
					if (!existing && !editor.isActive(this.name)) {
						return commands.setMark(this.name, { style: styles });
					}

					const items = {
						...parseStyles(existing),
						...parseStyles(styles),
					};

					const style = serializeStyles(items);
					if (style === ';') return false;

					return commands.updateAttributes(this.name, { style });
				},
			toggleSpanStyle:
				(styles) =>
				({ commands, editor }) => {
					if (!styles) return false;
					const existing = editor.getAttributes(this.name)?.style as string;
					return existing?.includes(styles) === true ? commands.unsetSpanStyle(styles) : commands.setSpanStyle(styles);
				},
			unsetSpanStyle:
				(styles) =>
				({ commands, editor }) => {
					if (!styles) return false;

					parseStyles(styles);

					const toBeRemoved = new Set<string>();

					styles
						.split(';')
						.map((x) => x.trim())
						.filter((x) => x)
						.forEach((rule) => {
							const [key] = rule.split(':');
							if (key) toBeRemoved.add(key.trim());
						});

					if (toBeRemoved.size === 0) return false;

					const existing = editor.getAttributes(this.name)?.style as string;
					const items = parseStyles(existing);

					// Remove keys
					for (const key of toBeRemoved) {
						delete items[key];
					}

					const style = serializeStyles(items);

					return style === ';'
						? commands.resetAttributes(this.name, 'style')
						: commands.updateAttributes(this.name, { style });
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		span: {
			setSpanStyle: (styles?: string) => ReturnType;
			toggleSpanStyle: (styles?: string) => ReturnType;
			unsetSpanStyle: (styles?: string) => ReturnType;
		};
	}
}
