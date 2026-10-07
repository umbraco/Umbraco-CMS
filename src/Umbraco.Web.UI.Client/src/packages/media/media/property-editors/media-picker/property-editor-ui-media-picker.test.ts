import { UmbPropertyEditorUIMediaPickerElement } from './property-editor-ui-media-picker.element.js';
import { manifests } from './manifests.js';
import type { UmbInputRichMediaElement } from '../../components/input-rich-media/input-rich-media.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbPropertyEditorUIMediaPickerElement', () => {
	let element: UmbPropertyEditorUIMediaPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-media-picker></umb-property-editor-ui-media-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMediaPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('clear', () => {
		it('should register a clear property action for the media picker', () => {
			const clearAction = manifests.find(
				(manifest) =>
					manifest.type === 'propertyAction' &&
					'kind' in manifest &&
					manifest.kind === 'clear' &&
					'forPropertyEditorUis' in manifest &&
					manifest.forPropertyEditorUis.includes('Umb.PropertyEditorUi.MediaPicker'),
			);
			expect(clearAction).to.not.be.undefined;
		});

		it('should show no items once the value is cleared', async () => {
			element.value = [{ key: 'entry-1', mediaKey: 'media-1', mediaTypeAlias: 'Image', focalPoint: null, crops: [] }];
			await element.updateComplete;

			element.value = undefined;
			await element.updateComplete;

			const input = element.shadowRoot!.querySelector<UmbInputRichMediaElement>('umb-input-rich-media')!;
			expect(input.value).to.deep.equal([]);
		});
	});
});
