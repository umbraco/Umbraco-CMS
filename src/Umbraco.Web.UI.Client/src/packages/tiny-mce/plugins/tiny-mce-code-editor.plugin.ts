import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '../components/input-tiny-mce/tiny-mce-plugin.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_CODE_EDITOR_MODAL } from '@umbraco-cms/backoffice/code-editor';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export default class UmbTinyMceCodeEditorPlugin extends UmbTinyMcePluginBase {
	constructor(args: TinyMcePluginArguments) {
		super(args);
		const localize = new UmbLocalizationController(args.host);

		this.editor.ui.registry.addButton('sourcecode', {
			icon: 'sourcecode',
			tooltip: localize.term('general_viewSourceCode'),
			onAction: () => this.#showCodeEditor(),
		});
	}

	async #showCodeEditor() {
		const value = await umbOpenModal(this, UMB_CODE_EDITOR_MODAL, {
			data: {
				headline: 'Edit source code',
				content: this.editor.getContent() ?? '',
				language: 'html',
			},
		}).catch(() => undefined);

		if (!value) {
			return;
		}

		if (!value.content) {
			this.editor.resetContent();
		} else {
			this.editor.setContent(value.content.toString());
		}

		this.editor.dispatch('Change');
	}
}
