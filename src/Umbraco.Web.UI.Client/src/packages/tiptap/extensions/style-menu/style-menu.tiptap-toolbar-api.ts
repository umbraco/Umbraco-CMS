import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { hasClassNames } from '../../utils/class-names.function.js';
import type { MetaTiptapToolbarStyleMenuItem } from '../../extensions/types.js';
import type { ChainedCommands, Editor } from '../../externals.js';

type UmbTiptapToolbarStyleMenuCommandType = {
	type: string;
	command?: (chain: ChainedCommands) => ChainedCommands;
	isActive?: (editor?: Editor) => boolean | undefined;
};

export default class UmbTiptapToolbarStyleMenuApi extends UmbTiptapToolbarElementApiBase {
	#headingCommand(level: 1 | 2 | 3 | 4 | 5 | 6): UmbTiptapToolbarStyleMenuCommandType {
		return {
			type: 'heading',
			command: (chain) => chain.toggleHeading({ level }),
			isActive: (editor) => editor?.isActive('heading', { level }),
		};
	}

	readonly #commands: Record<string, UmbTiptapToolbarStyleMenuCommandType> = {
		h1: this.#headingCommand(1),
		h2: this.#headingCommand(2),
		h3: this.#headingCommand(3),
		h4: this.#headingCommand(4),
		h5: this.#headingCommand(5),
		h6: this.#headingCommand(6),
		p: { type: 'paragraph', command: (chain) => chain.setParagraph() },
		a: { type: 'umbLink', command: (chain) => chain.ensureUmbLink({ type: 'external', href: '#' }) },
		blockquote: { type: 'blockquote', command: (chain) => chain.toggleBlockquote() },
		code: { type: 'code', command: (chain) => chain.toggleCode() },
		codeBlock: { type: 'codeBlock', command: (chain) => chain.toggleCodeBlock() },
		div: { type: 'div', command: (chain) => chain.toggleNode('div', 'paragraph') },
		em: { type: 'italic', command: (chain) => chain.toggleItalic() },
		ol: { type: 'orderedList', command: (chain) => chain.toggleOrderedList() },
		strong: { type: 'bold', command: (chain) => chain.toggleBold() },
		s: { type: 'strike', command: (chain) => chain.toggleStrike() },
		span: { type: 'span', command: (chain) => chain.toggleMark('span') },
		u: { type: 'underline', command: (chain) => chain.toggleUnderline() },
		ul: { type: 'bulletList', command: (chain) => chain.toggleBulletList() },
	};

	override isActive(editor?: Editor, item?: MetaTiptapToolbarStyleMenuItem) {
		if (!editor || !item?.data) return false;

		const { tag, id, class: className } = item.data;
		if (tag) return this.#isTagActive(editor, tag, id, className);
		return this.#hasAncestorWithAttributes(editor, id, className);
	}

	#isTagActive(editor: Editor, tag: string, id?: string, className?: string): boolean {
		const ext = this.#commands[tag];
		if (!ext) return false;
		const tagMatch = ext.isActive?.(editor) ?? editor.isActive(ext.type) ?? false;
		return tagMatch && this.#hasAttributes(editor.getAttributes(ext.type), id, className);
	}

	#hasAncestorWithAttributes(editor: Editor, id?: string, className?: string): boolean {
		// Without a tag, `execute` toggles the id/class on every node type around the selection, not only on paragraphs,
		// so the item is active when any ancestor node of the selection carries them.
		const { $from } = editor.state.selection;
		for (let depth = $from.depth; depth > 0; depth--) {
			if (this.#hasAttributes($from.node(depth).attrs, id, className)) return true;
		}
		return false;
	}

	#hasAttributes(attrs: Record<string, unknown>, id?: string, className?: string): boolean {
		const idMatch = !id ? true : attrs.id === id;
		const classMatch = !className ? true : hasClassNames(attrs.class, className);
		return idMatch && classMatch;
	}

	override execute(editor?: Editor, item?: MetaTiptapToolbarStyleMenuItem) {
		if (!editor || !item?.data) return;
		const { tag, id, class: className } = item.data;
		const focus = editor.chain().focus();
		const ext = tag ? this.#commands[tag] : null;
		(ext?.command?.(focus) ?? focus).toggleId(id, ext?.type).toggleClassName(className, ext?.type).run();
	}
}
