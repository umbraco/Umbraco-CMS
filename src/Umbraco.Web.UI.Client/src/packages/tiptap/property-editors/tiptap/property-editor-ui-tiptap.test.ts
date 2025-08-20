import { UmbPropertyEditorUiTiptapElement } from './property-editor-ui-tiptap.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

describe('UmbPropertyEditorUITiptapElement', () => {
	let element: UmbPropertyEditorUiTiptapElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-tiptap></umb-property-editor-ui-tiptap> `);
	});

	afterEach(() => {
		element?.remove();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUiTiptapElement);
	});

	it('can set and get value with markup', async () => {
		const testValue = {
			markup: '<p>Test content</p>',
			blocks: {
				layout: {},
				contentData: [],
				settingsData: [],
				expose: []
			}
		};
		element.value = testValue;
		await element.updateComplete;
		expect(element.value).to.deep.equal(testValue);
	});

	it('handles undefined value gracefully', async () => {
		element.value = undefined;
		await element.updateComplete;
		expect(element.value).to.be.undefined;
	});

	it('can be set to readonly', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.readonly).to.be.true;
	});

	it('can be set to mandatory', async () => {
		element.mandatory = true;
		await element.updateComplete;
		expect(element.mandatory).to.be.true;
	});

	it('accepts configuration', async () => {
		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'extensions', value: ['Umb.Extension.Test'] }
		]);
		element.config = config;
		await element.updateComplete;
		expect(element.config).to.equal(config);
	});

	it('renders umb-input-tiptap element', async () => {
		await element.updateComplete;
		const inputElement = element.shadowRoot?.querySelector('umb-input-tiptap');
		expect(inputElement).to.exist;
	});

	it('handles change events from tiptap input', async () => {
		await element.updateComplete;
		
		// Mock the input element
		const inputElement = element.shadowRoot?.querySelector('umb-input-tiptap') as any;
		if (inputElement) {
			// Mock isEmpty method
			inputElement.isEmpty = () => false;
			inputElement.value = '<p>New content</p>';
			
			// Simulate change event
			const changeEvent = new CustomEvent('change', { 
				bubbles: true,
				detail: { value: '<p>New content</p>' }
			});
			Object.defineProperty(changeEvent, 'target', {
				value: inputElement,
				enumerable: true
			});
			
			inputElement.dispatchEvent(changeEvent);
			await element.updateComplete;
			
			// Verify the value was processed
			expect(element.value).to.exist;
			expect(element.value?.markup).to.equal('<p>New content</p>');
		}
	});

	it('clears value when input is empty', async () => {
		// Set initial value
		element.value = {
			markup: '<p>Test</p>',
			blocks: { layout: {}, contentData: [], settingsData: [], expose: [] }
		};
		await element.updateComplete;

		// Mock the input element as empty
		const inputElement = element.shadowRoot?.querySelector('umb-input-tiptap') as any;
		if (inputElement) {
			inputElement.isEmpty = () => true;
			inputElement.value = '';
			
			// Simulate change event
			const changeEvent = new CustomEvent('change', { 
				bubbles: true,
				detail: { value: '' }
			});
			Object.defineProperty(changeEvent, 'target', {
				value: inputElement,
				enumerable: true
			});
			
			inputElement.dispatchEvent(changeEvent);
			await element.updateComplete;
			
			// Verify the value was cleared
			expect(element.value).to.be.undefined;
		}
	});

	it('processes block elements in markup', async () => {
		await element.updateComplete;
		
		const markupWithBlocks = '<p>Content</p><umb-rte-block data-content-key="test-key-123"><!--Umbraco-Block--></umb-rte-block><p>More content</p>';
		
		const inputElement = element.shadowRoot?.querySelector('umb-input-tiptap') as any;
		if (inputElement) {
			inputElement.isEmpty = () => false;
			inputElement.value = markupWithBlocks;
			
			const changeEvent = new CustomEvent('change', { 
				bubbles: true,
				detail: { value: markupWithBlocks }
			});
			Object.defineProperty(changeEvent, 'target', {
				value: inputElement,
				enumerable: true
			});
			
			inputElement.dispatchEvent(changeEvent);
			await element.updateComplete;
			
			// Verify the markup was preserved
			expect(element.value?.markup).to.equal(markupWithBlocks);
		}
	});

	it('applies CSS styles for validation states', async () => {
		await element.updateComplete;
		
		// The element should have styles for invalid state
		const styles = UmbPropertyEditorUiTiptapElement.styles;
		expect(styles).to.exist;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
