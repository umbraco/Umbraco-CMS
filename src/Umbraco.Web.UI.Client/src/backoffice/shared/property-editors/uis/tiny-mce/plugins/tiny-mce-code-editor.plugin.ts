import { TinyMcePluginArguments, TinyMcePluginBase } from './tiny-mce-plugin';

export class TinyMceCodeEditorPlugin extends TinyMcePluginBase {
	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.editor.ui.registry.addButton('ace', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.modalContext?.codeEditor({
			headline: 'Edit source code',
			content: this.editor.getContent() ?? '',
		});

		if (!modalHandler) return;

		const { confirmed, content } = await modalHandler.onClose();
		if (!confirmed) return;

		this.editor.setContent(content);
		this.editor.dispatch('Change');
	}
}
