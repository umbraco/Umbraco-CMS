import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_CODE_EDITOR_MODAL } from '@umbraco-cms/backoffice/code-editor';

export default class UmbTiptapToolbarSourceEditorExtensionApi extends UmbTiptapToolbarElementApiBase {
	#localize = new UmbLocalizationController(this);

	override async execute(editor?: Editor) {
		if (!editor) return;

		const data = await umbOpenModal(this, UMB_CODE_EDITOR_MODAL, {
			data: {
				headline: this.#localize.term('tiptap_sourceCodeEdit'),
				content: editor?.getHTML() ?? '',
				language: 'html',
				formatOnLoad: true,
			},
		}).catch(() => undefined);

		if (!data) return;

		editor?.commands.setContent(data.content);
	}
}
