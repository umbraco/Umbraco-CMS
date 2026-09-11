import { Blockquote, Document, Editor, Paragraph, Text } from '../../externals.js';
import { HtmlClassAttribute } from './html-attr-class.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('HtmlClassAttribute', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [
				Document,
				Paragraph,
				Text,
				Blockquote,
				HtmlClassAttribute.configure({ types: ['blockquote', 'paragraph'] }),
			],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('toggles a class fully off when it is set on an ancestor rather than the selected node', () => {
		editor.commands.setContent('<blockquote class="callout"><p>text</p></blockquote>');
		editor.commands.setTextSelection(2);

		editor.commands.toggleClassName('callout');

		expect(editor.getHTML()).to.not.include('class="callout"');
	});

	it('applies a class to every configured type around the selection when none of them has it yet', () => {
		editor.commands.setContent('<blockquote><p>text</p></blockquote>');
		editor.commands.setTextSelection(2);

		editor.commands.toggleClassName('callout');

		expect(editor.getHTML()).to.include('<blockquote class="callout"><p class="callout">');
	});
});
