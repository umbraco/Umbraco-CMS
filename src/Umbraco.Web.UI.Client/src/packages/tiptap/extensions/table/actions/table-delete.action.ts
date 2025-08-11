import { UmbTiptapMenuItemActionApi } from '../../../components/menu/tiptap-menu-item-api-base.js';
import { UMB_TIPTAP_RTE_CONTEXT } from '../../../contexts/tiptap-rte.context-token.js';

export default class UmbTableDeleteAction extends UmbTiptapMenuItemActionApi {
	override async execute() {
		const ctx = await this.getContext(UMB_TIPTAP_RTE_CONTEXT);
		ctx?.getEditor()?.chain().focus().deleteTable().run();
	}
}
