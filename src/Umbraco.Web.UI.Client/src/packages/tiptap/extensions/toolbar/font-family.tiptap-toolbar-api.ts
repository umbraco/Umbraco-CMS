import { UmbTiptapToolbarElementApiBase } from '../base.js';
import type { MetaTiptapToolbarMenuItem } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarFontFamilyExtensionApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		if (!item?.data) return;
		editor?.chain().focus().setSpanStyle(`font-family: ${item.data};`).run();
	}
}
