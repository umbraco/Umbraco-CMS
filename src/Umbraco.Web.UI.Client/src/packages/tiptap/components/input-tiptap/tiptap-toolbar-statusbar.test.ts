import { UmbTiptapToolbarElement } from './tiptap-toolbar.element.js';
import { UmbTiptapStatusbarElement } from './tiptap-statusbar.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

// Mock editor for testing
const createMockEditor = (): Editor => ({
	isActive: () => false,
	can: () => ({ run: () => true }),
	chain: () => ({
		focus: () => ({ run: () => true })
	})
} as any);

describe('UmbTiptapToolbarElement', () => {
	let element: UmbTiptapToolbarElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-tiptap-toolbar></umb-tiptap-toolbar> `);
	});

	afterEach(() => {
		element?.remove();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTiptapToolbarElement);
	});

	it('has default toolbar configuration', () => {
		expect(element.toolbar).to.be.an('array');
		expect(element.toolbar).to.deep.equal([[[]]]);
	});

	it('can set and get readonly state', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.readonly).to.be.true;
		
		element.readonly = false;
		await element.updateComplete;
		expect(element.readonly).to.be.false;
	});

	it('can set and get editor', async () => {
		const mockEditor = createMockEditor();
		element.editor = mockEditor;
		await element.updateComplete;
		expect(element.editor).to.equal(mockEditor);
	});

	it('can set and get configuration', async () => {
		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'toolbar', value: [['bold', 'italic']] }
		]);
		element.configuration = config;
		await element.updateComplete;
		expect(element.configuration).to.equal(config);
	});

	it('can set and get toolbar configuration', async () => {
		const toolbarConfig = [['bold', 'italic'], ['link', 'unlink']];
		element.toolbar = toolbarConfig;
		await element.updateComplete;
		expect(element.toolbar).to.deep.equal(toolbarConfig);
	});

	it('handles complex toolbar configuration', async () => {
		const complexToolbarConfig = [
			[['bold', 'italic'], ['underline', 'strike']],
			[['link'], ['media-picker']],
			[['undo', 'redo']]
		];
		element.toolbar = complexToolbarConfig;
		await element.updateComplete;
		expect(element.toolbar).to.deep.equal(complexToolbarConfig);
	});

	it('reflects readonly attribute', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.hasAttribute('readonly')).to.be.true;
		
		element.readonly = false;
		await element.updateComplete;
		expect(element.hasAttribute('readonly')).to.be.false;
	});

	it('handles disconnection gracefully', () => {
		document.body.appendChild(element);
		document.body.removeChild(element);
		// Should not throw any errors during disconnection
		expect(true).to.be.true;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});

describe('UmbTiptapStatusbarElement', () => {
	let element: UmbTiptapStatusbarElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-tiptap-statusbar></umb-tiptap-statusbar> `);
	});

	afterEach(() => {
		element?.remove();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTiptapStatusbarElement);
	});

	it('has default statusbar configuration', () => {
		expect(element.statusbar).to.be.an('array');
		expect(element.statusbar).to.deep.equal([[], []]);
	});

	it('can set and get readonly state', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.readonly).to.be.true;
		
		element.readonly = false;
		await element.updateComplete;
		expect(element.readonly).to.be.false;
	});

	it('can set and get editor', async () => {
		const mockEditor = createMockEditor();
		element.editor = mockEditor;
		await element.updateComplete;
		expect(element.editor).to.equal(mockEditor);
	});

	it('can set and get configuration', async () => {
		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'statusbar', value: ['word-count', 'character-count'] }
		]);
		element.configuration = config;
		await element.updateComplete;
		expect(element.configuration).to.equal(config);
	});

	it('handles string statusbar configuration', () => {
		element.statusbar = 'word-count' as any;
		expect(element.statusbar).to.deep.equal([[], ['word-count']]);
	});

	it('handles single array statusbar configuration', () => {
		element.statusbar = [['word-count']];
		expect(element.statusbar).to.deep.equal([[], ['word-count']]);
	});

	it('handles full array statusbar configuration', () => {
		const statusbarConfig = [['element-path'], ['word-count', 'character-count']];
		element.statusbar = statusbarConfig;
		expect(element.statusbar).to.deep.equal(statusbarConfig);
	});

	it('reflects readonly attribute', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.hasAttribute('readonly')).to.be.true;
		
		element.readonly = false;
		await element.updateComplete;
		expect(element.hasAttribute('readonly')).to.be.false;
	});

	it('handles disconnection gracefully', () => {
		document.body.appendChild(element);
		document.body.removeChild(element);
		// Should not throw any errors during disconnection
		expect(true).to.be.true;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});