import { Document, Editor, Paragraph, Text } from '../../externals.js';
import { UmbLink } from './link.tiptap-extension.js';
import { expect } from '@open-wc/testing';

describe('link.tiptap-extension', () => {
	let editor: Editor;
	let host: HTMLDivElement;

	beforeEach(() => {
		host = document.createElement('div');
		document.body.appendChild(host);
		editor = new Editor({
			element: host,
			extensions: [Document, Paragraph, Text, UmbLink],
		});
	});

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

	it('strips a pre-existing data-router-slot attribute from legacy stored content (regression)', () => {
		editor.commands.setContent('<p><a href="https://example.com" data-router-slot="disabled">link</a></p>');

		expect(editor.getHTML()).to.not.include('data-router-slot');

		const anchor = host.querySelector('a');
		expect(anchor?.dataset.routerSlot).to.equal('disabled');
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
});
