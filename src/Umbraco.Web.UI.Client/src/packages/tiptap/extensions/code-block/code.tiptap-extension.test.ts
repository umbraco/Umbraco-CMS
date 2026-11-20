import { Bold, Document, Editor, Paragraph, Text } from '../../externals.js';
import { UmbLink } from '../link/link.tiptap-extension.js';
import { UmbCode } from './code.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('UmbCode', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({ element: host, extensions: [Document, Paragraph, Text, Bold, UmbLink, UmbCode] });
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('lets a link wrap inline code', () => {
		editor.commands.setContent('<p><code>snippet</code></p>');
		editor.chain().selectAll().setUmbLink({ type: 'external', href: 'https://example.com' }).run();

		expect(editor.getHTML()).to.equal('<p><a href="https://example.com"><code>snippet</code></a></p>');
	});

	it('wraps text and inline code in a single link', () => {
		editor.commands.setContent('<p>See <code>snippet</code> here</p>');
		editor.chain().selectAll().setUmbLink({ type: 'external', href: 'https://example.com' }).run();

		expect(editor.getHTML()).to.equal('<p><a href="https://example.com">See <code>snippet</code> here</a></p>');
	});

	it('keeps a link around inline code when the content is loaded', () => {
		const html = '<p><a href="https://example.com"><code>snippet</code></a></p>';

		editor.commands.setContent(html);

		expect(editor.getHTML()).to.equal(html);
	});

	it('still keeps other formatting out of inline code', () => {
		editor.commands.setContent('<p><code>snippet</code></p>');
		editor.chain().selectAll().setBold().run();

		expect(editor.getHTML()).to.equal('<p><code>snippet</code></p>');
	});
});
