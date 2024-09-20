import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHeading3ExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Heading];

	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 3 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 3 }).run();
	}
}
