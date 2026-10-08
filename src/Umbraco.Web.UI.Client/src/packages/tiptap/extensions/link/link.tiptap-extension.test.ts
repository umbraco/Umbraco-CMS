import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { HtmlDatasetAttributes } from '../html-attr-dataset/html-attr-dataset.tiptap-extension.js';
import { UmbLink } from './link.tiptap-extension.js';
import { expect } from '@open-wc/testing';
import type { AnyExtension } from '../../externals.js';

describe('UmbLink', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	function createEditor(extensions: Array<AnyExtension>) {
		editor?.destroy();
		host?.remove();
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({ element: host, extensions: [Document, Paragraph, Text, UmbLink, ...extensions] });
	}

	beforeEach(() => createEditor([]));

	afterEach(() => {
		editor.destroy();
		host.remove();
	});

	it('sets data-router-slot="disabled" on the live rendered anchor', () => {
		editor.commands.setContent('<p><a href="https://example.com">link</a></p>');

		const anchor = host.querySelector('a');
		expect(anchor?.dataset.routerSlot).to.equal('disabled');
	});

	it('does not leak data-router-slot into the serialized HTML (regression)', () => {
		editor.commands.setContent('<p><a href="https://example.com">link</a></p>');

		expect(editor.getHTML()).to.not.include('data-router-slot');
	});

	it('preserves other attributes on both the live anchor and the serialized HTML', () => {
		editor.commands.setContent('<p><a href="https://example.com" target="_blank">link</a></p>');

		const anchor = host.querySelector('a');
		expect(anchor?.getAttribute('href')).to.equal('https://example.com');
		expect(anchor?.getAttribute('target')).to.equal('_blank');
		expect(editor.getHTML()).to.include('href="https://example.com"');
		expect(editor.getHTML()).to.include('target="_blank"');
	});

	it('hosts the wrapped inline content inside the live anchor', () => {
		editor.commands.setContent('<p><a href="https://example.com">link</a></p>');

		const anchor = host.querySelector('a');
		expect(anchor?.textContent).to.equal('link');
	});

	it('does not default rel/target from the parent Link extension (regression)', () => {
		editor.commands.setContent('<p><a href="https://example.com">link</a></p>');

		const anchor = host.querySelector('a');
		expect(anchor?.hasAttribute('rel')).to.be.false;
		expect(anchor?.hasAttribute('target')).to.be.false;
		expect(editor.getHTML()).to.not.include('rel=');
	});

	// The `data-*` attributes extension is part of the default rich text editor configuration, and it round-trips
	// every `data-*` attribute it finds. Without it, these tests pass no matter what `data-router-slot` does.
	describe('with the `data-*` attributes extension enabled', () => {
		beforeEach(() => createEditor([HtmlDatasetAttributes.configure({ types: ['paragraph', 'umbLink'] })]));

		it('strips a pre-existing data-router-slot attribute from legacy stored content (regression)', () => {
			editor.commands.setContent('<p><a href="https://example.com" data-router-slot="disabled">link</a></p>');

			expect(editor.getHTML()).to.not.include('data-router-slot');

			const anchor = host.querySelector('a');
			expect(anchor?.dataset.routerSlot).to.equal('disabled');
		});

		it('does not absorb data-router-slot when the live anchor is re-parsed by the schema (regression)', () => {
			editor.commands.setContent('<p><a href="https://example.com">link</a></p>');
			editor.commands.setContent(host.querySelector('.tiptap')?.innerHTML ?? '');

			expect(editor.getHTML()).to.not.include('data-router-slot');
		});

		it('still round-trips authored data-* attributes', () => {
			editor.commands.setContent('<p><a href="https://example.com" data-foo="bar">link</a></p>');

			expect(editor.getHTML()).to.include('data-foo="bar"');
		});
	});
});
