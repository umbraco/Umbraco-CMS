import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHeading2ExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Heading];

	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 2 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 2 }).run();
	}
}
