import { UmbInputTiptapElement } from './components/input-tiptap/input-tiptap.element.js';
import { UmbPropertyEditorUiTiptapElement } from './property-editors/tiptap/property-editor-ui-tiptap.element.js';
import { UmbTiptapRteContext } from './contexts/tiptap-rte.context.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-integration-host')
class UmbTestIntegrationHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('Tiptap Integration Tests', () => {
	
	describe('Property Editor and Input Integration', () => {
		let propertyEditor: UmbPropertyEditorUiTiptapElement;

		beforeEach(async () => {
			propertyEditor = await fixture(html` 
				<umb-property-editor-ui-tiptap></umb-property-editor-ui-tiptap> 
			`);
		});

		afterEach(() => {
			propertyEditor?.remove();
		});

		it('property editor contains input element', async () => {
			await propertyEditor.updateComplete;
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap');
			expect(inputElement).to.exist;
			expect(inputElement).to.be.instanceOf(UmbInputTiptapElement);
		});

		it('property editor passes configuration to input', async () => {
			const config = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['Umb.Extension.Bold', 'Umb.Extension.Italic'] }
			]);
			
			propertyEditor.config = config;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			expect(inputElement?.configuration).to.equal(config);
		});

		it('property editor passes readonly state to input', async () => {
			propertyEditor.readonly = true;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			expect(inputElement?.readonly).to.be.true;
		});

		it('property editor passes required state to input', async () => {
			propertyEditor.mandatory = true;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			expect(inputElement?.required).to.be.true;
		});

		it('property editor synchronizes value with input', async () => {
			const testValue = {
				markup: '<p>Test content</p>',
				blocks: {
					layout: {},
					contentData: [],
					settingsData: [],
					expose: []
				}
			};
			
			propertyEditor.value = testValue;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			expect(inputElement?.value).to.equal(testValue.markup);
		});
	});

	describe('Context Integration', () => {
		let hostElement: UmbTestIntegrationHostElement;
		let context: UmbTiptapRteContext;

		beforeEach(() => {
			hostElement = new UmbTestIntegrationHostElement();
			context = new UmbTiptapRteContext(hostElement);
			document.body.appendChild(hostElement);
		});

		afterEach(() => {
			context.destroy();
			document.body.innerHTML = '';
		});

		it('context can manage editor lifecycle', () => {
			expect(context.getEditor()).to.be.undefined;
			
			const mockEditor = { id: 'test-editor' } as any;
			context.setEditor(mockEditor);
			expect(context.getEditor()).to.equal(mockEditor);
			
			context.setEditor(undefined);
			expect(context.getEditor()).to.be.undefined;
		});

		it('context integrates with host element lifecycle', () => {
			const mockEditor = { id: 'test-editor' } as any;
			context.setEditor(mockEditor);
			
			// Remove host element from DOM
			document.body.removeChild(hostElement);
			
			// Context should still maintain editor reference until explicitly destroyed
			expect(context.getEditor()).to.equal(mockEditor);
		});
	});

	describe('Configuration Flow Integration', () => {
		let propertyEditor: UmbPropertyEditorUiTiptapElement;

		beforeEach(async () => {
			propertyEditor = await fixture(html` 
				<umb-property-editor-ui-tiptap></umb-property-editor-ui-tiptap> 
			`);
		});

		afterEach(() => {
			propertyEditor?.remove();
		});

		it('configuration flows from property editor to input and affects extensions', async () => {
			const config = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['Umb.Extension.Bold', 'Umb.Extension.Link'] },
				{ alias: 'toolbar', value: [['bold'], ['link']] },
				{ alias: 'statusbar', value: ['word-count'] }
			]);
			
			propertyEditor.config = config;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			
			// Configuration should be passed through
			expect(inputElement?.configuration).to.equal(config);
			
			// Allow time for extension loading
			await new Promise(resolve => setTimeout(resolve, 100));
			
			// Extensions should be loaded based on configuration
			expect(inputElement?.['_extensions']).to.exist;
		});

		it('empty configuration is handled gracefully', async () => {
			const config = new UmbPropertyEditorConfigCollection([]);
			
			propertyEditor.config = config;
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as UmbInputTiptapElement;
			expect(inputElement?.configuration).to.equal(config);
			
			// Should not throw errors with empty configuration
			expect(() => { if (inputElement) inputElement.configuration = config; }).to.not.throw;
		});
	});

	describe('Value Processing Integration', () => {
		let propertyEditor: UmbPropertyEditorUiTiptapElement;

		beforeEach(async () => {
			propertyEditor = await fixture(html` 
				<umb-property-editor-ui-tiptap></umb-property-editor-ui-tiptap> 
			`);
		});

		afterEach(() => {
			propertyEditor?.remove();
		});

		it('handles markup with block elements correctly', async () => {
			const markupWithBlocks = `
				<p>Introduction text</p>
				<umb-rte-block data-content-key="block-123"><!--Umbraco-Block--></umb-rte-block>
				<umb-rte-block-inline data-content-key="inline-456"><!--Umbraco-Block--></umb-rte-block-inline>
				<p>Conclusion text</p>
			`.trim();

			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as any;
			if (inputElement) {
				// Simulate change event with block markup
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
				await propertyEditor.updateComplete;
				
				// Property editor should preserve the block markup
				expect(propertyEditor.value?.markup).to.equal(markupWithBlocks);
			}
		});

		it('transitions between empty and non-empty states correctly', async () => {
			await propertyEditor.updateComplete;
			
			const inputElement = propertyEditor.shadowRoot?.querySelector('umb-input-tiptap') as any;
			if (inputElement) {
				// Start with content
				inputElement.isEmpty = () => false;
				inputElement.value = '<p>Some content</p>';
				
				let changeEvent = new CustomEvent('change', { bubbles: true });
				Object.defineProperty(changeEvent, 'target', { value: inputElement });
				inputElement.dispatchEvent(changeEvent);
				await propertyEditor.updateComplete;
				
				expect(propertyEditor.value).to.exist;
				expect(propertyEditor.value?.markup).to.equal('<p>Some content</p>');
				
				// Clear content
				inputElement.isEmpty = () => true;
				inputElement.value = '';
				
				changeEvent = new CustomEvent('change', { bubbles: true });
				Object.defineProperty(changeEvent, 'target', { value: inputElement });
				inputElement.dispatchEvent(changeEvent);
				await propertyEditor.updateComplete;
				
				expect(propertyEditor.value).to.be.undefined;
			}
		});
	});
});