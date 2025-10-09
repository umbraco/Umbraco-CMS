import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { MetaTiptapToolbarMenuItem } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarFontFamilyExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		const styles = editor?.getAttributes('span')?.style;
		return styles?.includes(`font-family: ${item?.data};`) === true;
	}

	override execute(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		if (!item?.data) return;
		editor?.chain().focus().toggleSpanStyle(`font-family: ${item.data};`).run();
	}
}
