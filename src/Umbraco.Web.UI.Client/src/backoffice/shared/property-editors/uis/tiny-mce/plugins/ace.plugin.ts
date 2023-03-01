import { UmbModalService } from '@umbraco-cms/modal';

// TODO => TinyMCE type definitions
export class AcePlugin {
	#modalService?: UmbModalService;
	editor?: any;

	constructor(editor: any, modalService?: UmbModalService) {
		this.#modalService = modalService;
		this.editor = editor;

		editor.ui.registry.addButton('ace', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.#modalService?.codeEditor({
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
