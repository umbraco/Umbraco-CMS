import { UmbCodeEditorModalData, UmbCodeEditorModalResult, UMB_CODE_EDITOR_MODAL_TOKEN } from '../../../../modals/code-editor';
import { TinyMcePluginArguments, TinyMcePluginBase } from './tiny-mce-plugin';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

export default class TinyMceCodeEditorPlugin extends TinyMcePluginBase {

	#modalContext?: UmbModalContext;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.host.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance: UmbModalContext) => {
			this.#modalContext = instance;
		});

		this.editor.ui.registry.addButton('ace', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.#modalContext?.open<UmbCodeEditorModalData, UmbCodeEditorModalResult>(UMB_CODE_EDITOR_MODAL_TOKEN, {
			headline: 'Edit source code',
			content: this.editor.getContent() ?? '',
			language: 'html',
		});

		if (!modalHandler) return;

		const { content } = await modalHandler.onSubmit();
		if (!content) {
			this.editor.resetContent();
		} else {
			this.editor.setContent(content.toString());
		}

		this.editor.dispatch('Change');
	}
}
