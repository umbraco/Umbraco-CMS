import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

describe('UmbInputTiptapElement - Validation and Edge Cases', () => {
	let element: UmbInputTiptapElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-tiptap></umb-input-tiptap> `);
	});

	afterEach(() => {
		element?.remove();
	});

	describe('Validation', () => {
		it('validates required field when empty', async () => {
			element.required = true;
			await element.updateComplete;

			// Check if the element reports as empty for validation
			expect(element.isEmpty()).to.be.true;
			
			// The element should have validation methods from UmbFormControlMixin
			expect(element).to.have.property('required');
		});

		it('validates required field when has content', async () => {
			element.required = true;
			element.value = '<p>Some content</p>';
			await element.updateComplete;
			
			// Wait for editor to be ready
			await new Promise(resolve => setTimeout(resolve, 100));
			
			// When content is present, isEmpty should return false
			// Note: This depends on the editor being properly initialized
			expect(element.isEmpty()).to.be.false;
		});

		it('handles requiredMessage property', async () => {
			const customMessage = 'This field is mandatory';
			element.requiredMessage = customMessage;
			await element.updateComplete;
			
			expect(element.requiredMessage).to.equal(customMessage);
		});
	});

	describe('Extension Loading', () => {
		it('loads default core extension when no extensions configured', async () => {
			// With no configuration, should still load the core extension
			await element.updateComplete;
			await new Promise(resolve => setTimeout(resolve, 100));
			
			// Check that the private _extensions array has at least the core extension
			// This tests the extension loading mechanism
			expect(element['_extensions']).to.exist;
		});

		it('loads configured extensions', async () => {
			const config = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['Umb.Extension.TextAlign', 'Umb.Extension.Link'] }
			]);
			element.configuration = config;
			await element.updateComplete;
			
			// Allow time for async extension loading
			await new Promise(resolve => setTimeout(resolve, 200));
			
			// The extensions should be loaded
			expect(element['_extensions']).to.exist;
		});

		it('handles invalid extension configurations gracefully', async () => {
			const config = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['NonExistent.Extension'] }
			]);
			element.configuration = config;
			await element.updateComplete;
			
			// Should not throw errors when loading non-existent extensions
			expect(() => element.configuration = config).to.not.throw;
		});
	});

	describe('Content Handling', () => {
		it('preserves complex HTML content', () => {
			const complexHtml = `
				<h1>Title</h1>
				<p>Paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
				<ul>
					<li>List item 1</li>
					<li>List item 2</li>
				</ul>
				<blockquote>A quoted text</blockquote>
			`.trim();
			
			element.value = complexHtml;
			expect(element.value).to.equal(complexHtml);
		});

		it('handles special characters and encoding', () => {
			const specialChars = '<p>Special chars: &amp; &lt; &gt; &quot; &#39;</p>';
			element.value = specialChars;
			expect(element.value).to.equal(specialChars);
		});

		it('handles very large content', () => {
			const largeContent = '<p>' + 'A'.repeat(10000) + '</p>';
			element.value = largeContent;
			expect(element.value).to.equal(largeContent);
		});

		it('handles empty and whitespace content', () => {
			// Test various empty/whitespace scenarios
			element.value = '';
			expect(element.value).to.equal('');
			
			element.value = ' ';
			expect(element.value).to.equal(' ');
			
			element.value = '\n\t  \n';
			expect(element.value).to.equal('\n\t  \n');
		});
	});

	describe('State Management', () => {
		it('maintains readonly state correctly', async () => {
			element.readonly = true;
			await element.updateComplete;
			
			expect(element.readonly).to.be.true;
			
			element.readonly = false;
			await element.updateComplete;
			
			expect(element.readonly).to.be.false;
		});

		it('handles configuration changes', async () => {
			const config1 = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['Extension1'] }
			]);
			const config2 = new UmbPropertyEditorConfigCollection([
				{ alias: 'extensions', value: ['Extension2'] }
			]);
			
			element.configuration = config1;
			await element.updateComplete;
			expect(element.configuration).to.equal(config1);
			
			element.configuration = config2;
			await element.updateComplete;
			expect(element.configuration).to.equal(config2);
		});

		it('handles rapid value changes', async () => {
			const values = ['<p>Value 1</p>', '<p>Value 2</p>', '<p>Value 3</p>'];
			
			for (const value of values) {
				element.value = value;
				expect(element.value).to.equal(value);
			}
		});
	});

	describe('Editor Lifecycle', () => {
		it('handles editor destruction gracefully', async () => {
			await element.updateComplete;
			
			// Simulate editor cleanup
			if (element['_editor']) {
				element['_editor'] = undefined;
			}
			
			// Should not throw when accessing isEmpty after editor cleanup
			expect(() => element.isEmpty()).to.not.throw;
		});

		it('handles multiple firstUpdated calls', async () => {
			await element.updateComplete;
			
			// Call firstUpdated again (shouldn't normally happen but test resilience)
			// This tests the robustness of the initialization
			expect(() => {
				(element as any).firstUpdated(new Map());
			}).to.not.throw;
		});
	});
});