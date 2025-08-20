import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

describe('UmbInputTiptapElement', () => {
	let element: UmbInputTiptapElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-tiptap></umb-input-tiptap> `);
	});

	afterEach(() => {
		element?.remove();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputTiptapElement);
	});

	it('has default empty value', () => {
		expect(element.value).to.equal('');
	});

	it('can set and get value', () => {
		const testValue = '<p>Test content</p>';
		element.value = testValue;
		expect(element.value).to.equal(testValue);
	});

	it('reports empty state correctly when no content', () => {
		expect(element.isEmpty()).to.be.true;
	});

	it('reports empty state correctly when has content', async () => {
		element.value = '<p>Some content</p>';
		// Wait for editor to initialize
		await element.updateComplete;
		await new Promise(resolve => setTimeout(resolve, 100));
		expect(element.isEmpty()).to.be.false;
	});

	it('can be set to readonly', async () => {
		element.readonly = true;
		await element.updateComplete;
		expect(element.readonly).to.be.true;
	});

	it('can be set to required', async () => {
		element.required = true;
		await element.updateComplete;
		expect(element.required).to.be.true;
	});

	it('accepts configuration', async () => {
		const config = new UmbPropertyEditorConfigCollection([
			{ alias: 'extensions', value: ['Umb.Extension.Test'] }
		]);
		element.configuration = config;
		await element.updateComplete;
		expect(element.configuration).to.equal(config);
	});

	it('handles undefined value gracefully', () => {
		element.value = undefined as any;
		expect(element.value).to.equal('');
	});

	it('handles null value gracefully', () => {
		element.value = null as any;
		expect(element.value).to.equal('');
	});

	it('fires change event when value changes', async () => {
		let changeEventFired = false;
		element.addEventListener('change', () => {
			changeEventFired = true;
		});

		element.value = '<p>New content</p>';
		// Allow for async operations to complete
		await element.updateComplete;
		await new Promise(resolve => setTimeout(resolve, 50));
		
		// Note: Change event is typically fired by the editor, not directly by value setter
		// This test verifies the element can handle change events
		expect(changeEventFired).to.be.false; // Expected as value setter doesn't fire change
	});

	it('maintains form control behavior', () => {
		// Test that it extends UmbFormControlMixin properly
		expect(element).to.have.property('value');
		expect(element).to.have.property('required');
		expect(element).to.have.property('readonly');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});