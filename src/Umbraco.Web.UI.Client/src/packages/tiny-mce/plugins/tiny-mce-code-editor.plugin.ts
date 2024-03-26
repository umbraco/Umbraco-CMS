import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '../components/input-tiny-mce/tiny-mce-plugin.js';
import { UMB_CODE_EDITOR_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbTinyMceCodeEditorPlugin extends UmbTinyMcePluginBase {
	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.editor.ui.registry.addButton('sourcecode', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_CODE_EDITOR_MODAL, {
			data: {
				headline: 'Edit source code',
				content: this.editor.getContent() ?? '',
				language: 'html',
			},
		});

		if (!modal) return;

		const { content } = await modal.onSubmit();
		if (!content) {
			this.editor.resetContent();
		} else {
			this.editor.setContent(content.toString());
		}

		this.editor.dispatch('Change');
	}
}
