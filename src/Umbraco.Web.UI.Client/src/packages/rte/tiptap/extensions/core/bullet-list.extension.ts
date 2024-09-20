import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { BulletList, ListItem } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBulletListExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [BulletList, ListItem];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBulletList().run();
	}
}
