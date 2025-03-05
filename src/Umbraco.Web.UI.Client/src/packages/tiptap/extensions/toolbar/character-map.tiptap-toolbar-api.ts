import { UmbTiptapToolbarElementApiBase } from '../base.js';
import { UMB_CHARACTER_MAP_MODAL } from '../../components/character-map/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarCharacterMapExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		if (!editor) return;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_CHARACTER_MAP_MODAL);

		if (!modal) return;

		const data = await modal.onSubmit().catch(() => undefined);
		if (!data) return;

		editor?.chain().focus().insertContent(data).run();
	}
}
