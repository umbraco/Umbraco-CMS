import type { Editor } from '../../externals.js';
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { Anchor } from './anchor.tiptap-extension.js';
import { UMB_TIPTAP_ANCHOR_MODAL } from './modals/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbTiptapToolbarAnchorExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const attrs = editor?.getAttributes(Anchor.name);
		if (!attrs) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager not found');
		const modal = modalManager.open(this, UMB_TIPTAP_ANCHOR_MODAL, { data: { id: attrs?.id } });
		if (!modal) return;

		const result = await modal.onSubmit().catch(() => undefined);
		if (!result) return;

		editor
			?.chain()
			.insertContent({ type: Anchor.name, attrs: { id: result } })
			.run();
	}
}
