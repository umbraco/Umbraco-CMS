import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '../components/input-tiny-mce/tiny-mce-plugin.js';
import type {
	UmbCodeEditorModalData,
	UmbCodeEditorModalValue,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import { UMB_CODE_EDITOR_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbTinyMceCodeEditorPlugin extends UmbTinyMcePluginBase {
	#modalContext?: UmbModalManagerContext;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (modalContext) => {
			this.#modalContext = modalContext;
		});

		this.editor.ui.registry.addButton('sourcecode', {
			icon: 'sourcecode',
			tooltip: 'View Source Code',
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const modalHandler = this.#modalContext?.open<UmbCodeEditorModalData, UmbCodeEditorModalValue>(
			UMB_CODE_EDITOR_MODAL,
			{
				data: {
					headline: 'Edit source code',
					content: this.editor.getContent() ?? '',
					language: 'html',
				},
			},
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
