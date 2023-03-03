import { Editor } from 'tinymce';
import { UmbModalContext } from '@umbraco-cms/modal';

export class TinyMceEmbeddedMediaPlugin {
	#modalContext: UmbModalContext;
	editor: Editor;

	constructor(editor: Editor, modalContext: UmbModalContext) {
		this.#modalContext = modalContext;
		this.editor = editor;

		editor.ui.registry.addButton('ace', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.#modalContext?.codeEditor({
			headline: 'Edit source code',
			content: this.editor.getContent(),
		});

		if (!modalHandler) return;

		const { confirmed, content } = await modalHandler.onClose();
		if (!confirmed) return;

		this.editor.setContent(content);
		this.editor.dispatch('Change');
	}
}
