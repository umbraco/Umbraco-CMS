import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { UMB_CODE_EDITOR_MODAL } from '@umbraco-cms/backoffice/code-editor';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export default class UmbTiptapToolbarSourceEditorExtensionApi extends UmbTiptapToolbarElementApiBase {
	#localize = new UmbLocalizationController(this);

	override async execute(editor?: Editor) {
		if (!editor) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_CODE_EDITOR_MODAL, {
			data: {
				headline: this.#localize.term('tiptap_sourceCodeEdit'),
				content: editor?.getHTML() ?? '',
				language: 'html',
				formatOnLoad: true,
			},
		});

		if (!modal) return;

		const data = await modal.onSubmit().catch(() => undefined);
		if (!data) return;

		editor?.commands.setContent(data.content, true);
	}
}
