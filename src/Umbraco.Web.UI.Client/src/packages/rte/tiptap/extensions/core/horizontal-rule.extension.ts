import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { HorizontalRule } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHorizontalRuleExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [HorizontalRule];

	override execute(editor?: Editor) {
		editor?.chain().focus().setHorizontalRule().run();
	}
}
