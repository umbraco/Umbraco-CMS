import { Mark, mergeAttributes } from '@tiptap/core';

export interface SpanOptions {
	/**
	 * HTML attributes to add to the span element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Span = Mark.create<SpanOptions>({
	name: 'span',

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

					const rules = ((existing ?? '') + ';' + styles).split(';');
					const items: Record<string, string> = {};

					rules
						.filter((x) => x)
						.forEach((rule) => {
							if (rule.trim() !== '') {
								const [key, value] = rule.split(':');
								items[key.trim()] = value.trim();
							}
						});

					const style = Object.entries(items)
						.map(([key, value]) => `${key}: ${value}`)
						.join(';');

					return commands.updateAttributes(this.name, { style });
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		span: {
			setSpanStyle: (styles?: string) => ReturnType;
		};
	}
}
