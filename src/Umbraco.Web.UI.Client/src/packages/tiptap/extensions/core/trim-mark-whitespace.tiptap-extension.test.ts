import { Bold, Document, Editor, Italic, Paragraph, Text } from '../../externals.js';
import { UmbTrimMarkWhitespace } from './trim-mark-whitespace.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('UmbTrimMarkWhitespace', () => {
	let editorHost: HTMLDivElement;
	let editor: Editor;

	beforeEach(() => {
		editorHost = document.createElement('div');
		document.body.appendChild(editorHost);
		editor = new Editor({
			element: editorHost,
			extensions: [Document, Paragraph, Text, Bold, Italic, UmbTrimMarkWhitespace],
		});
	});

	afterEach(() => {
		editor.destroy();
		editorHost.remove();
	});

	const select = (html: string, from: number, to: number) => {
		editor.commands.setContent(html);
		editor.commands.setTextSelection({ from, to });
	};

	it('excludes trailing whitespace from an applied mark', () => {
		select('<p>This is a bold word</p>', 11, 16);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('excludes leading whitespace from an applied mark', () => {
		select('<p>This is a bold word</p>', 10, 15);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('applies to any mark, not only bold', () => {
		select('<p>This is a bold word</p>', 11, 16);
		editor.commands.toggleItalic();
		expect(editor.getHTML()).to.equal('<p>This is a <em>bold</em> word</p>');
	});

	it('marks a whitespace-only selection unchanged', () => {
		select('<p>a b</p>', 2, 3);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>a<strong> </strong>b</p>');
	});

	it('leaves a selection without edge whitespace unchanged', () => {
		select('<p>This is a bold word</p>', 11, 15);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a <strong>bold</strong> word</p>');
	});

	it('still removes a mark from the full selection', () => {
		select('<p>This is a <strong>bold </strong>word</p>', 11, 16);
		editor.commands.toggleBold();
		expect(editor.getHTML()).to.equal('<p>This is a bold word</p>');
	});
});
