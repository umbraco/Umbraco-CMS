import { Editor } from 'tinymce';
import { TinyMcePluginArguments } from '../../../../components/input-tiny-mce/input-tiny-mce.element';
import { UmbModalContext } from '@umbraco-cms/modal';

export class TinyMceCodeEditorPlugin {
	editor: Editor;
	#modalContext?: UmbModalContext;

	constructor(args: TinyMcePluginArguments) {
		this.#modalContext = args.modalContext;
		this.editor = args.editor;

		this.editor.ui.registry.addButton('ace', {
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
