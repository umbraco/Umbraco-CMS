import {
	Blockquote,
	Bold,
	BulletList,
	Document,
	Editor,
	Heading,
	Image,
	ListItem,
	Paragraph,
	Text,
} from '../../externals.js';
import { HtmlClassAttribute } from '../html-attr-class/html-attr-class.tiptap-extension.js';
import { HtmlIdAttribute } from '../html-attr-id/html-attr-id.tiptap-extension.js';
import type { MetaTiptapToolbarStyleMenuItem } from '../types.js';
import UmbTiptapToolbarStyleMenuApi from './style-menu.tiptap-toolbar-api.js';
import { expect } from '@open-wc/testing';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-style-menu-host')
class UmbTestStyleMenuHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbTiptapToolbarStyleMenuApi', () => {
	let hostElement: UmbTestStyleMenuHostElement;
	let editorHost: HTMLDivElement;
	let editor: Editor;
	let api: UmbTiptapToolbarStyleMenuApi;

	const attributeTypes = ['blockquote', 'bold', 'bulletList', 'heading', 'image', 'listItem', 'paragraph'];

	const item = (data: MetaTiptapToolbarStyleMenuItem['data']): MetaTiptapToolbarStyleMenuItem => ({
		label: 'Test style',
		data,
	});

	/** Loads the HTML and places the caret at `position` (by default at the start of the first text block). */
	const setContent = (html: string, position = 1) => {
		editor.commands.setContent(html);
		editor.commands.setTextSelection(position);
	};

	beforeEach(() => {
		hostElement = new UmbTestStyleMenuHostElement();
		document.body.appendChild(hostElement);

		editorHost = document.createElement('div');
		document.body.appendChild(editorHost);

		editor = new Editor({
			element: editorHost,
			extensions: [
				Document,
				Paragraph,
				Text,
				Heading,
				BulletList,
				ListItem,
				Bold,
				Image,
				Blockquote,
				HtmlClassAttribute.configure({ types: attributeTypes }),
				HtmlIdAttribute.configure({ types: attributeTypes }),
			],
		});

		api = new UmbTiptapToolbarStyleMenuApi(hostElement);
	});

	afterEach(() => {
		api.destroy();
		editor.destroy();
		editorHost.remove();
		hostElement.remove();
	});

	describe('isActive', () => {
		it('detects a class-only style on a paragraph', () => {
			setContent('<p class="lead">text</p>');

			expect(api.isActive(editor, item({ class: 'lead' }))).to.equal(true);
		});

		it('detects a class-only style on a heading', () => {
			setContent('<h2 class="title--size-5">text</h2>');

			expect(api.isActive(editor, item({ class: 'title--size-5' }))).to.equal(true);
		});

		it('detects a class-only style on an ancestor of the selection', () => {
			// Caret inside the paragraph of the list item; the class sits on the list itself.
			setContent('<ul class="list--checkmarks"><li><p>item</p></li></ul>', 3);

			expect(api.isActive(editor, item({ class: 'list--checkmarks' }))).to.equal(true);
		});

		it('detects an id-only style on a heading', () => {
			setContent('<h2 id="intro">text</h2>');

			expect(api.isActive(editor, item({ id: 'intro' }))).to.equal(true);
			expect(api.isActive(editor, item({ id: 'outro' }))).to.equal(false);
		});

		it('is not active when the class is absent', () => {
			setContent('<h2>text</h2>');

			expect(api.isActive(editor, item({ class: 'title--size-5' }))).to.equal(false);
		});

		it('compares whole class names', () => {
			setContent('<p class="title--size-10">text</p>');

			expect(api.isActive(editor, item({ class: 'title--size-10' }))).to.equal(true);
			expect(api.isActive(editor, item({ class: 'title--size-1' }))).to.equal(false);
		});

		it('requires every class name of a multi-class style', () => {
			setContent('<p class="list list--ordered">text</p>');

			expect(api.isActive(editor, item({ class: 'list list--ordered' }))).to.equal(true);
			expect(api.isActive(editor, item({ class: 'list--ordered list' }))).to.equal(true);
			expect(api.isActive(editor, item({ class: 'list list--unordered' }))).to.equal(false);
		});

		it('checks the tag together with the class', () => {
			setContent('<h2 class="title">text</h2>');

			expect(api.isActive(editor, item({ tag: 'h2', class: 'title' }))).to.equal(true);
			expect(api.isActive(editor, item({ tag: 'h3', class: 'title' }))).to.equal(false);
			expect(api.isActive(editor, item({ tag: 'h2', class: 'other' }))).to.equal(false);
		});

		it('reflects a class-only style after it has been applied to a heading', () => {
			setContent('<h2>text</h2>');

			api.execute(editor, item({ class: 'title--size-5' }));

			expect(editor.getHTML()).to.include('<h2 class="title--size-5">');
			expect(api.isActive(editor, item({ class: 'title--size-5' }))).to.equal(true);
		});

		it('checks the class together with a tag that maps to a mark', () => {
			setContent('<p><strong class="loud">text</strong></p>', 2);

			expect(api.isActive(editor, item({ tag: 'strong', class: 'loud' }))).to.equal(true);
			expect(api.isActive(editor, item({ tag: 'strong', class: 'quiet' }))).to.equal(false);
		});

		it('is not active for a tag that has no matching command', () => {
			setContent('<p class="loud">text</p>');

			expect(api.isActive(editor, item({ tag: 'aside', class: 'loud' }))).to.equal(false);
		});

		it('detects a class-only style on a mark at the selection', () => {
			setContent('<p><strong class="loud">text</strong></p>', 2);

			expect(api.isActive(editor, item({ class: 'loud' }))).to.equal(true);
			expect(api.isActive(editor, item({ class: 'quiet' }))).to.equal(false);
		});

		it('detects a class-only style on a node-selected image', () => {
			editor.commands.setContent('<p>text</p><img class="framed" src="test.png">');
			editor.commands.setNodeSelection(6);

			expect(api.isActive(editor, item({ class: 'framed' }))).to.equal(true);
			expect(api.isActive(editor, item({ class: 'unframed' }))).to.equal(false);
		});
	});

	describe('execute', () => {
		it('turns a class-only style fully off after one toggle, even split across an ancestor and its content', () => {
			setContent('<blockquote class="callout"><p>text</p></blockquote>', 2);
			expect(api.isActive(editor, item({ class: 'callout' }))).to.equal(true);

			api.execute(editor, item({ class: 'callout' }));

			expect(api.isActive(editor, item({ class: 'callout' }))).to.equal(false);
		});
	});
});
