import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { UMB_TIPTAP_ANCHOR_MODAL } from '../../components/anchor-modal/index.js';
import { Anchor } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarAnchorExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const attrs = editor?.getAttributes(Anchor.name);
		if (!attrs) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
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
