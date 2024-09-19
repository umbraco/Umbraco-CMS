import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Bold } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Bold];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBold().run();
	}
}
