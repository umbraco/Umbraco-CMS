import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { MetaTiptapToolbarMenuItem } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapToolbarFontSizeExtensionApi extends UmbTiptapToolbarElementApiBase {
	override isActive(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		const styles = editor?.getAttributes('span')?.style;
		return styles?.includes(`font-size: ${item?.data};`) === true;
	}

	override execute(editor?: Editor, item?: MetaTiptapToolbarMenuItem) {
		if (!item?.data) return;
		editor?.chain().focus().toggleSpanStyle(`font-size: ${item.data};`).run();
	}
}
