import { Bold, Document, Editor, Mark, Paragraph, Text } from '../../externals.js';
import UmbTiptapToolbarClearFormattingExtensionApi from './clear-formatting.tiptap-toolbar-api.js';
import { expect } from '@open-wc/testing';
import { UmbLink } from '../link/link.tiptap-extension.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

// A minimal mark extension that opts out of "Clear formatting" via Tiptap's own `clearable: false`
// contract, so tests can assert the contract itself rather than a specific mark alias.
const UmbTestNonClearableMark = Mark.create({
	name: 'nonClearable',
	clearable: false,
	parseHTML: () => [{ tag: 'mark' }],
	renderHTML: () => ['mark', {}, 0],
});

@customElement('umb-test-clear-formatting-host')
class UmbTestClearFormattingHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTiptapToolbarClearFormattingExtensionApi', () => {
	let hostElement: UmbTestClearFormattingHostElement;
	let editorHost: HTMLDivElement;
	let editor: Editor;
	let api: UmbTiptapToolbarClearFormattingExtensionApi;

	beforeEach(() => {
		hostElement = new UmbTestClearFormattingHostElement();
		document.body.appendChild(hostElement);

		editorHost = document.createElement('div');
		document.body.appendChild(editorHost);

		editor = new Editor({
			element: editorHost,
			extensions: [Document, Paragraph, Text, Bold, UmbLink, UmbTestNonClearableMark],
		});

		api = new UmbTiptapToolbarClearFormattingExtensionApi(hostElement);
	});

	afterEach(() => {
		api.destroy();
		editor.destroy();
		editorHost.remove();
		hostElement.remove();
	});

	it('clears character formatting while preserving links', () => {
		editor.commands.setContent('<p><strong><a href="https://example.com">link</a></strong></p>');
		editor.commands.selectAll();

		api.execute(editor);

		const html = editor.getHTML();
		expect(html).to.include('href="https://example.com"');
		expect(html).to.not.include('<strong>');
	});

	it('clears character formatting when no link is present', () => {
		editor.commands.setContent('<p><strong>bold text</strong></p>');
		editor.commands.selectAll();

		api.execute(editor);

		expect(editor.getHTML()).to.not.include('<strong>');
	});

	it('preserves link attributes when clearing formatting', () => {
		editor.commands.setContent(
			'<p><strong><a href="https://example.com" target="_blank" title="Example" type="external">link</a></strong></p>',
		);
		editor.commands.selectAll();

		api.execute(editor);

		const html = editor.getHTML();
		expect(html).to.include('href="https://example.com"');
		expect(html).to.include('target="_blank"');
		expect(html).to.include('title="Example"');
		expect(html).to.include('type="external"');
	});

	it('preserves any mark declaring clearable: false, not just links', () => {
		editor.commands.setContent('<p><strong><mark>marked text</mark></strong></p>');
		editor.commands.selectAll();

		api.execute(editor);

		const html = editor.getHTML();
		expect(html).to.include('<mark>');
		expect(html).to.not.include('<strong>');
	});
});
