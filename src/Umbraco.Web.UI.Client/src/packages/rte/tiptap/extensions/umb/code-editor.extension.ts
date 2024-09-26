import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { UMB_CODE_EDITOR_MODAL } from '@umbraco-cms/backoffice/code-editor';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbTiptapCodeEditorExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [];

	override async execute(editor?: Editor) {
		console.log('umb-code-editor.execute', editor);
		if (!editor) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_CODE_EDITOR_MODAL, {
			data: {
				headline: 'Edit source code',
				content: editor?.getHTML() ?? '',
				language: 'html',
			},
		});

		if (!modal) return;

		const data = await modal.onSubmit().catch(() => undefined);
		if (!data) return;

		editor?.commands.setContent(data.content, true);
	}
}
