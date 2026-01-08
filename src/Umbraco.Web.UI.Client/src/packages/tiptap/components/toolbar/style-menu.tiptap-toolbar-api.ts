import { UmbTiptapToolbarElementApiBase } from '../../extensions/base.js';
import type { MetaTiptapToolbarStyleMenuItem } from '../../extensions/types.js';
import type { ChainedCommands, Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarStyleMenuApi extends UmbTiptapToolbarElementApiBase {
	#commands: Record<string, { type: string; command: (chain: ChainedCommands) => ChainedCommands }> = {
		h1: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 1 }) },
		h2: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 2 }) },
		h3: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 3 }) },
		h4: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 4 }) },
		h5: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 5 }) },
		h6: { type: 'heading', command: (chain) => chain.toggleHeading({ level: 6 }) },
		p: { type: 'paragraph', command: (chain) => chain.setParagraph() },
		blockquote: { type: 'blockquote', command: (chain) => chain.toggleBlockquote() },
		code: { type: 'code', command: (chain) => chain.toggleCode() },
		codeBlock: { type: 'codeBlock', command: (chain) => chain.toggleCodeBlock() },
		div: { type: 'div', command: (chain) => chain.toggleNode('div', 'paragraph') },
		em: { type: 'italic', command: (chain) => chain.setItalic() },
		ol: { type: 'orderedList', command: (chain) => chain.toggleOrderedList() },
		strong: { type: 'bold', command: (chain) => chain.setBold() },
		s: { type: 'strike', command: (chain) => chain.setStrike() },
		span: { type: 'span', command: (chain) => chain.toggleMark('span') },
		u: { type: 'underline', command: (chain) => chain.setUnderline() },
		ul: { type: 'bulletList', command: (chain) => chain.toggleBulletList() },
	};

	override execute(editor?: Editor, item?: MetaTiptapToolbarStyleMenuItem) {
		if (!editor || !item?.data) return;
		const { tag, id, class: className } = item.data;
		const focus = editor.chain().focus();
		const ext = tag ? this.#commands[tag] : null;
		(ext?.command?.(focus) ?? focus).setId(id, ext?.type).setClassName(className, ext?.type).run();
	}
}
