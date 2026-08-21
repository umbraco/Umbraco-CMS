import { Bold, Document, Editor, Paragraph, Text } from '../../externals.js';
import UmbTiptapToolbarClearFormattingExtensionApi from './clear-formatting.tiptap-toolbar-api.js';
import { expect } from '@open-wc/testing';
import { UmbLink } from '../link/link.tiptap-extension.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-clear-formatting-host')
class UmbTestClearFormattingHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('clear-formatting.tiptap-toolbar-api', () => {
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
			extensions: [Document, Paragraph, Text, Bold, UmbLink],
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
});
