import { BulletList, Document, Editor, ListItem, Paragraph, Text } from '../../externals.js';
import { Div } from './html-tag-div.tiptap-extension.js';
import { DivContainer } from './html-tag-div-container.tiptap-extension.js';
import { HtmlClassAttribute } from '../html-attr-class/html-attr-class.tiptap-extension.js';
import { HtmlStyleAttribute } from '../html-attr-style/html-attr-style.tiptap-extension.js';
import { umbRteBlock, umbRteBlockInline } from '../block/block.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('html-tag-div-container.tiptap-extension', () => {
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
				BulletList,
				ListItem,
				Div,
				DivContainer,
				umbRteBlock,
				umbRteBlockInline,
				HtmlClassAttribute.configure({ types: ['div', 'divContainer'] }),
				HtmlStyleAttribute.configure({ types: ['div', 'divContainer'] }),
			],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('preserves a paragraph nested inside a div (regression for #22654)', () => {
		editor.commands.setContent('<div><p>hello</p></div>');

		expect(editor.getHTML()).to.equal('<div><p>hello</p></div>');
	});

	it('preserves multiple block children nested inside a div', () => {
		// The stock Tiptap ListItem node always wraps its content in a <p> (out of scope for #22654);
		// what this asserts is that the <ul> itself stays nested inside the <div> rather than being lifted out.
		editor.commands.setContent('<div><p>a</p><ul><li>b</li></ul></div>');

		expect(editor.getHTML()).to.equal('<div><p>a</p><ul><li><p>b</p></li></ul></div>');
	});

	it('preserves a div containing only inline text, without injecting a paragraph', () => {
		editor.commands.setContent('<div>plain text</div>');

		expect(editor.getHTML()).to.equal('<div>plain text</div>');
	});

	it('preserves an empty div, without injecting a paragraph', () => {
		editor.commands.setContent('<div></div>');

		expect(editor.getHTML()).to.equal('<div></div>');
	});

	it('preserves nested block-container divs', () => {
		editor.commands.setContent('<div><div><p>x</p></div></div>');

		expect(editor.getHTML()).to.equal('<div><div><p>x</p></div></div>');
	});

	it('preserves the class attribute on a block-container div', () => {
		editor.commands.setContent('<div class="box"><p>x</p></div>');

		expect(editor.getHTML()).to.equal('<div class="box"><p>x</p></div>');
	});

	it('preserves an umb-rte-block nested inside a div', () => {
		editor.commands.setContent('<div><umb-rte-block data-content-key="key-1"></umb-rte-block></div>');

		expect(editor.getHTML()).to.equal('<div><umb-rte-block data-content-key="key-1"></umb-rte-block></div>');
	});

	it('falls through to the inline div for a div wrapping only an umb-rte-block-inline', () => {
		editor.commands.setContent('<div><umb-rte-block-inline data-content-key="key-1"></umb-rte-block-inline></div>');

		expect(editor.getHTML()).to.equal(
			'<div><umb-rte-block-inline data-content-key="key-1"></umb-rte-block-inline></div>',
		);
		// Both `div` and `divContainer` render as <div>, so assert on the parsed node type directly
		// to confirm the inline div (not divContainer) claimed the element.
		expect(editor.state.doc.firstChild?.type.name).to.equal('div');
	});

	it('wraps stray inline content in a paragraph when a div mixes inline and block children', () => {
		editor.commands.setContent('<div>text<p>a</p></div>');

		const html = editor.getHTML();
		expect(html).to.equal('<div><p>text</p><p>a</p></div>');

		// Re-parsing the normalised output must be idempotent - no further drift on subsequent saves.
		editor.commands.setContent(html);
		expect(editor.getHTML()).to.equal(html);
	});
});
