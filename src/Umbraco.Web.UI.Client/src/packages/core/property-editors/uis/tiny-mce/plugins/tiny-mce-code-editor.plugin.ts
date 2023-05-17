import { TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbCodeEditorModalData,
	UmbCodeEditorModalResult,
	UMB_CODE_EDITOR_MODAL,
} from '@umbraco-cms/backoffice/modal';

export default class UmbTinyMceCodeEditorPlugin extends UmbTinyMcePluginBase {
	#modalContext?: UmbModalContext;

	constructor(args: TinyMcePluginArguments) {		
		super(args);

		this.host.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (modalContext) => {
			this.#modalContext = modalContext;
		});

		this.editor.ui.registry.addButton('ace', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.#modalContext?.open<UmbCodeEditorModalData, UmbCodeEditorModalResult>(
			UMB_CODE_EDITOR_MODAL,
			{
				headline: 'Edit source code',
				content: this.editor.getContent() ?? '',
				language: 'html',
			}
		);

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
