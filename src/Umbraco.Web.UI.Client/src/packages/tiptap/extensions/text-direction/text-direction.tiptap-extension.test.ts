import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { TextDirection } from './text-direction.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('TextDirection', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			enableCoreExtensions: { textDirection: false },
			extensions: [Document, Paragraph, Text, TextDirection],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('does not emit dir when no direction has been set', () => {
		editor.commands.setContent('<p>Hello</p>');

		expect(editor.getHTML()).to.not.include('dir=');
	});

	it('persists an explicitly set direction', () => {
		editor.commands.setContent('<p>Hello</p>');
		editor.commands.setTextDirection('rtl', { from: 0, to: editor.state.doc.content.size });

		expect(editor.getHTML()).to.include('dir="rtl"');
	});

	it('persists an explicitly set direction (ltr)', () => {
		editor.commands.setContent('<p>Hello</p>');
		editor.commands.setTextDirection('ltr', { from: 0, to: editor.state.doc.content.size });

		expect(editor.getHTML()).to.include('dir="ltr"');
	});

	it('persists an explicitly set direction (auto)', () => {
		editor.commands.setContent('<p>Hello</p>');
		editor.commands.setTextDirection('auto', { from: 0, to: editor.state.doc.content.size });

		expect(editor.getHTML()).to.include('dir="auto"');
	});

	it('round-trips an authored dir attribute', () => {
		editor.commands.setContent('<p dir="rtl">Hello</p>');

		expect(editor.getHTML()).to.include('dir="rtl"');
	});

	it('removes dir when the direction is unset', () => {
		editor.commands.setContent('<p>Hello</p>');
		const range = { from: 0, to: editor.state.doc.content.size };

		editor.commands.setTextDirection('rtl', range);
		expect(editor.getHTML()).to.include('dir="rtl"');

		editor.commands.unsetTextDirection(range);

		expect(editor.getHTML()).to.not.include('dir=');
	});

	it('reports the node as active for the direction that was set (regression)', () => {
		editor.commands.setContent('<p>Hello</p>');
		editor.commands.setTextDirection('rtl', { from: 0, to: editor.state.doc.content.size });

		expect(editor.isActive({ dir: 'rtl' })).to.equal(true);
	});
});
