import { UmbPropertyEditorUIContentPickerElement } from './property-editor-ui-content-picker.element.js';
import { manifests } from './manifests.js';
import type { UmbInputContentElement } from './components/input-content/index.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIContentPickerElement', () => {
	let element: UmbPropertyEditorUIContentPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIContentPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('clear', () => {
		it('should register a clear property action for the content picker', () => {
			const clearAction = manifests.find(
				(manifest) =>
					manifest.type === 'propertyAction' &&
					'kind' in manifest &&
					manifest.kind === 'clear' &&
					'forPropertyEditorUis' in manifest &&
					manifest.forPropertyEditorUis.includes('Umb.PropertyEditorUi.ContentPicker'),
			);
			expect(clearAction).to.not.be.undefined;
		});

		it('should show no items once the value is cleared', async () => {
			element.value = [{ type: 'document', unique: 'document-1' }];
			await element.updateComplete;

			element.value = undefined;
			await element.updateComplete;

			const input = element.shadowRoot!.querySelector<UmbInputContentElement>('umb-input-content')!;
			expect(input.selection).to.deep.equal([]);
		});
	});
});
