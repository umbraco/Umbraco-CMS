import { Bold, Document, Editor, Paragraph, Text } from '../../externals.js';
import UmbTiptapToolbarLinkExtensionApi from './link.tiptap-toolbar-api.js';
import { UmbLink } from './link.tiptap-extension.js';
import { expect } from '@open-wc/testing';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbLinkPickerLink } from '@umbraco-cms/backoffice/multi-url-picker';
import type { UmbModalContextClassArgs, UmbModalToken } from '@umbraco-cms/backoffice/modal';

// Immediately submits every modal with the link set on it, standing in for the editor using the link picker.
class UmbTestLinkPickerModalManagerContext extends UmbModalManagerContext {
	link?: UmbLinkPickerLink;

	override open<
		ModalData extends { [key: string]: any } = { [key: string]: any },
		ModalValue = unknown,
		ModalAliasTypeAsToken extends UmbModalToken = UmbModalToken<ModalData, ModalValue>,
	>(
		host: UmbControllerHost,
		modalAlias: UmbModalToken<ModalData, ModalValue> | string,
		args?: UmbModalContextClassArgs<ModalAliasTypeAsToken>,
	) {
		const modalContext = super.open<ModalData, ModalValue, ModalAliasTypeAsToken>(host, modalAlias, args);
		modalContext.setValue({ link: this.link } as ModalValue);
		modalContext.submit();
		return modalContext;
	}
}

@customElement('umb-test-link-toolbar-host')
class UmbTestLinkToolbarHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	modalManager = new UmbTestLinkPickerModalManagerContext(this);
}

describe('UmbTiptapToolbarLinkExtensionApi', () => {
	let hostElement: UmbTestLinkToolbarHostElement;
	let editorHost: HTMLDivElement;
	let editor: Editor;
	let api: UmbTiptapToolbarLinkExtensionApi;

	const pickLink = (link: UmbLinkPickerLink) => {
		hostElement.modalManager.link = { type: 'external', ...link };
		return api.execute(editor);
	};

	const marksOn = (text: string) => {
		const names: Array<string> = [];
		editor.state.doc.descendants((node) => {
			if (node.isText && node.text === text) names.push(...node.marks.map((mark) => mark.type.name));
		});
		return names;
	};

	beforeEach(() => {
		hostElement = new UmbTestLinkToolbarHostElement();
		document.body.appendChild(hostElement);

		editorHost = document.createElement('div');
		document.body.appendChild(editorHost);

		editor = new Editor({ element: editorHost, extensions: [Document, Paragraph, Text, Bold, UmbLink] });

		api = new UmbTiptapToolbarLinkExtensionApi(hostElement);
	});

	afterEach(() => {
		api.destroy();
		editor.destroy();
		editorHost.remove();
		hostElement.remove();
	});

	describe('when no text is selected', () => {
		beforeEach(() => {
			editor.commands.setContent('<p></p>');
			editor.commands.setTextSelection(1);
		});

		it('inserts the link at the cursor, using its name as the text', async () => {
			await pickLink({ name: 'Read more', url: 'https://example.com' });

			expect(editor.getHTML()).to.equal('<p><a href="https://example.com" title="Read more">Read more</a></p>');
		});

		it('inserts the link using its URL as the text when it has no name', async () => {
			await pickLink({ name: '', url: 'https://example.com' });

			const anchor = editorHost.querySelector('a');
			expect(anchor?.getAttribute('href')).to.equal('https://example.com');
			expect(anchor?.textContent).to.equal('https://example.com');
		});

		it('does not extend the inserted link to text typed after it', async () => {
			await pickLink({ name: 'Read more', url: 'https://example.com' });

			editor.view.dispatch(editor.state.tr.insertText(' today'));

			expect(editorHost.querySelector('a')?.textContent).to.equal('Read more');
			expect(editorHost.textContent).to.equal('Read more today');
		});

		it('inserts a document link using its URL, not its local link, as the text when it has no name', async () => {
			const unique = 'eed5fc6b-96fd-45a5-a0f1-b1adfb483c2f';
			await pickLink({ name: '', type: 'document', unique, url: '/about-us/' });

			const anchor = editorHost.querySelector('a');
			expect(anchor?.getAttribute('href')).to.equal(`/{localLink:${unique}}`);
			expect(anchor?.textContent).to.equal('/about-us/');
		});

		it('keeps the formatting at the cursor on the link and the text typed after it', async () => {
			editor.commands.setContent('<p><strong>Bold</strong></p>');
			editor.commands.setTextSelection(5);

			await pickLink({ name: 'Read more', url: 'https://example.com' });
			editor.view.dispatch(editor.state.tr.insertText(' today'));

			expect(marksOn('Read more')).to.have.members(['bold', UmbLink.name]);
			expect(marksOn(' today')).to.deep.equal(['bold']);
		});
	});

	it('applies the link to the selected text without changing it', async () => {
		editor.commands.setContent('<p>Find out more</p>');
		editor.commands.setTextSelection({ from: 10, to: 14 });

		await pickLink({ name: 'Read more', url: 'https://example.com' });

		expect(editor.getHTML()).to.equal('<p>Find out <a href="https://example.com" title="Read more">more</a></p>');
	});

	it('updates the link the cursor is in without inserting more text', async () => {
		editor.commands.setContent('<p>Find out <a href="https://example.com" title="Old">more</a></p>');
		editor.commands.setTextSelection(12);

		await pickLink({ name: 'New', url: 'https://example.org' });

		expect(editor.getHTML()).to.equal('<p>Find out <a href="https://example.org" title="New">more</a></p>');
	});
});
