import { UmbTiptapExtensionApi } from './types.js';
import type { ManifestTiptapExtension } from './tiptap-extension.js';
import { Link } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.UrlPicker',
	name: 'URL Picker Tiptap Extension',
	api: () => import('./urlpicker.extension.js'),
	meta: {
		alias: 'umb-link',
		icon: 'umbraco',
		label: 'URL picker',
	},
};

export default class UmbTiptapUrlPickerExtensionApi extends UmbTiptapExtensionApi {
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
