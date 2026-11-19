import { Extension } from '../../externals.js';
import { hasClassNames, splitClassNames } from '../../utils/class-names.function.js';
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

					const toggleClasses = splitClassNames(className);
					if (toggleClasses.length === 0) {
						return true;
					}

					// One decision for the whole command: deciding per type would let one type lose the classes while
					// another gains them, so the toggle would never settle on or off across a selection spanning both.
					const removeToggleClasses = types.some((t) =>
						hasClassNames(editor.getAttributes(t)?.class as string | undefined, className),
					);

					return types
						.map((t) => {
							const classes = splitClassNames(editor.getAttributes(t)?.class as string | undefined);
							const newClasses = removeToggleClasses
								? classes.filter((c) => !toggleClasses.includes(c))
								: [...classes, ...toggleClasses.filter((c) => !classes.includes(c))];

							if (newClasses.length === 0) {
								// No classes left, remove the attribute entirely
								return commands.resetAttributes(t, 'class');
							}
							return commands.updateAttributes(t, { class: newClasses.join(' ') });
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
