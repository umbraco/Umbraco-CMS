import { UmbTiptapToolbarElementApiBase } from './types.js';
import type { ManifestTiptapExtensionButtonKind } from './tiptap-extension.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_CODE_EDITOR_MODAL, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.CodeEditor',
	name: 'Code Editor Tiptap Extension',
	api: () => import('./code-editor.extension.js'),
	weight: 1000,
	meta: {
		alias: 'umb-code-editor',
		icon: 'icon-code',
		label: '#general_viewSourceCode',
	},
};

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
