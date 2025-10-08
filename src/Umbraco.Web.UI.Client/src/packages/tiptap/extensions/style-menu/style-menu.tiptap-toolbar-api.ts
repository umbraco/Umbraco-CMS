import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { MetaTiptapToolbarStyleMenuItem } from '../../extensions/types.js';
import type { ChainedCommands, Editor } from '@umbraco-cms/backoffice/external/tiptap';

type UmbTiptapToolbarStyleMenuCommandType = {
	type: string;
	command: (chain: ChainedCommands) => ChainedCommands;
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

	#commands: Record<string, UmbTiptapToolbarStyleMenuCommandType> = {
		h1: this.#headingCommand(1),
		h2: this.#headingCommand(2),
		h3: this.#headingCommand(3),
		h4: this.#headingCommand(4),
		h5: this.#headingCommand(5),
		h6: this.#headingCommand(6),
		p: { type: 'paragraph', command: (chain) => chain.setParagraph() },
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
		const ext = tag ? this.#commands[tag] : null;
		const attrs = editor?.getAttributes(ext?.type ?? 'paragraph');

		const tagMatch = !tag ? true : ext ? (ext.isActive?.(editor) ?? editor?.isActive(ext.type) ?? false) : false;
		const idMatch = !id ? true : attrs.id === id;
		const classMatch = !className ? true : attrs.class?.includes(className) === true;

		return tagMatch && idMatch && classMatch;
	}

	override execute(editor?: Editor, item?: MetaTiptapToolbarStyleMenuItem) {
		if (!editor || !item?.data) return;
		const { tag, id, class: className } = item.data;
		const focus = editor.chain().focus();
		const ext = tag ? this.#commands[tag] : null;
		(ext?.command?.(focus) ?? focus).toggleId(id, ext?.type).toggleClassName(className, ext?.type).run();
	}
}
