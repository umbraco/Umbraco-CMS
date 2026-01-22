import { Extension } from '../../externals.js';
import type { Attributes } from '../../externals.js';

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		htmlClassAttribute: {
			setClassName: (className?: string, type?: string) => ReturnType;
			toggleClassName: (className?: string, type?: string) => ReturnType;
			unsetClassName: (type?: string) => ReturnType;
		};
	}
}

export interface UmbTiptapHtmlClassAttributeOptions {
	types: Array<string>;
}

export const HtmlClassAttribute = Extension.create<UmbTiptapHtmlClassAttributeOptions>({
	name: 'htmlClassAttribute',

	addOptions() {
		return { types: [] };
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: { class: {} } as Attributes,
			},
		];
	},

	addCommands() {
		return {
			setClassName:
				(className, type) =>
				({ commands }) => {
					if (!className) return false;
					const types = type ? [type] : this.options.types;
					return types
						.map((type) => commands.updateAttributes(type, { class: className }))
						.every((response) => response);
				},
			toggleClassName:
				(className, type) =>
				({ commands, editor }) => {
					if (!className) return false;
					const types = type ? [type] : this.options.types;

					return types
						.map((t) => {
							const existingClass = (editor.getAttributes(t)?.class as string) ?? '';
							const classes = existingClass.split(/\s+/).filter((c) => c);
							const hasClass = classes.includes(className);

							if (hasClass) {
								// Remove the class
								const newClasses = classes.filter((c) => c !== className);
								if (newClasses.length === 0) {
									// No classes left, remove the attribute entirely
									return commands.resetAttributes(t, 'class');
								}
								return commands.updateAttributes(t, { class: newClasses.join(' ') });
							} else {
								// Add the class
								const newClasses = [...classes, className];
								return commands.updateAttributes(t, { class: newClasses.join(' ') });
							}
						})
						.every((response) => response);
				},
			unsetClassName:
				(type) =>
				({ commands }) => {
					const types = type ? [type] : this.options.types;
					return types.map((type) => commands.resetAttributes(type, 'class')).every((response) => response);
				},
		};
	},
});
