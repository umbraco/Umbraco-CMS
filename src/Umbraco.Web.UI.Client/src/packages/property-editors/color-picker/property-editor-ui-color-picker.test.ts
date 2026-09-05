import { UmbPropertyEditorUIColorPickerElement } from './property-editor-ui-color-picker.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

describe('UmbPropertyEditorUIColorPickerElement', () => {
	let element: UmbPropertyEditorUIColorPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-color-picker></umb-property-editor-ui-color-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIColorPickerElement);
	});

	describe('label sync', () => {
		const swatches = [
			{ value: '#28802a', label: 'Green' },
			{ value: '#ff0000', label: 'Red' },
		];

		const configWith = (items: Array<{ value: string; label: string }>) =>
			new UmbPropertyEditorConfigCollection([{ alias: 'items', value: items }]);

		it('updates value.label and emits a single change event when config label differs from stored label', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);

			element.config = configWith(swatches);
			element.value = { value: '#28802a', label: 'Forest' };
			await element.updateComplete;

			expect(element.value?.label).to.equal('Green');
			expect(element.value?.value).to.equal('#28802a');
			expect(changeCount).to.equal(1);
		});

		it('refreshes a stale label even when the stored hex casing differs from the swatch', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);

			element.config = configWith(swatches);
			element.value = { value: '#28802A', label: 'Forest' };
			await element.updateComplete;

			expect(element.value?.label).to.equal('Green');
			expect(changeCount).to.equal(1);
		});

		it('does not emit a change event when labels already match', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);

			element.config = configWith(swatches);
			element.value = { value: '#28802a', label: 'Green' };
			await element.updateComplete;

			expect(element.value?.label).to.equal('Green');
			expect(changeCount).to.equal(0);
		});

		it('does not emit a change event when no swatch matches the stored value', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);

			element.config = configWith(swatches);
			element.value = { value: '#000000', label: 'Black' };
			await element.updateComplete;

			expect(element.value?.label).to.equal('Black');
			expect(changeCount).to.equal(0);
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
