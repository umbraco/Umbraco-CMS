import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Strike } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapStrikeExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Strike];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleStrike().run();
	}
}
