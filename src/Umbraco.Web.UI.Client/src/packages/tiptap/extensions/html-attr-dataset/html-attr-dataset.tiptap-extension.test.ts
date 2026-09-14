import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { HtmlDatasetAttributes } from './html-attr-dataset.tiptap-extension.js';
import { UmbLink } from '../link/link.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('HtmlDatasetAttributes', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [Document, Paragraph, Text, UmbLink, HtmlDatasetAttributes.configure({ types: ['paragraph', 'umbLink'] })],
		});
	});

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('strips data-router-slot="disabled" from an anchor (regression)', () => {
		editor.commands.setContent('<p><a href="https://example.com" data-router-slot="disabled">link</a></p>');

		expect(editor.getHTML()).to.not.include('data-router-slot');
	});

	it('preserves data-router-slot on a non-anchor (regression)', () => {
		editor.commands.setContent('<p data-router-slot="disabled">text</p>');

		expect(editor.getHTML()).to.include('data-router-slot="disabled"');
	});

	it('preserves data-router-slot="enabled" on an anchor (regression)', () => {
		editor.commands.setContent('<p><a href="https://example.com" data-router-slot="enabled">link</a></p>');

		expect(editor.getHTML()).to.include('data-router-slot="enabled"');
	});

	it('still round-trips authored data-* attributes', () => {
		editor.commands.setContent('<p data-foo="bar">text</p>');

		expect(editor.getHTML()).to.include('data-foo="bar"');
	});
});
