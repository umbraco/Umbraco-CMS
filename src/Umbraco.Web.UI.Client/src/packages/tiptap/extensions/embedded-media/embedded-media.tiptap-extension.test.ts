import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { umbEmbeddedMedia } from './embedded-media.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('embedded-media.tiptap-extension', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [Document, Paragraph, Text, umbEmbeddedMedia.configure({ inline: true })],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('parses and serializes an embed without throwing (regression for the renderSpec RangeError)', () => {
		const markup =
			'<p><span class="umb-embed-holder" data-embed-constrain="false" data-embed-height="240" data-embed-url="https://example.com/embed" data-embed-width="360"><iframe src="https://example.com/embed" width="360" height="240"></iframe></span></p>';

		expect(() => editor.commands.setContent(markup)).to.not.throw();
		expect(editor.getHTML()).to.equal(markup);
	});

	it('does not render the literal text "null" when the markup attribute is unset', () => {
		editor.commands.insertContent({ type: 'umbEmbeddedMedia', attrs: { 'data-embed-url': 'https://example.com' } });

		expect(editor.getHTML()).to.not.include('null');
	});
});
