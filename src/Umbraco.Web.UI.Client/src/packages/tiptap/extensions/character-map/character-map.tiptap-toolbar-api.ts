import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { UMB_TIPTAP_CHARACTER_MAP_MODAL } from './modals/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarCharacterMapExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		if (!editor) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager not found');
		const modal = modalManager.open(this, UMB_TIPTAP_CHARACTER_MAP_MODAL);

		if (!modal) return;

		const data = await modal.onSubmit().catch(() => undefined);
		if (!data) return;

		editor?.chain().focus().insertContent(data).run();
	}
}
