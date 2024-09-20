import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { Link } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUrlPickerExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions() {
		return [Link.extend({ openOnClick: false })];
	}

	override async execute(editor?: Editor) {
		console.log('umb-link.execute', editor);

		const text = prompt('Enter the text');
		const url = prompt('Enter the URL');

		if (url && text && editor) {
			const { from } = editor.state.selection;
			editor
				.chain()
				.focus()
				.insertContent(text)
				.setTextSelection({ from: from, to: from + text.length })
				.setLink({ href: url, target: '_blank' })
				.run();
		}
	}
}
