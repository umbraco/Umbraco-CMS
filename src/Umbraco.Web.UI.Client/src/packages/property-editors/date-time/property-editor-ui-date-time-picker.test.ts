import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import UmbPropertyEditorUIDateTimePickerElement from './date-time-picker/property-editor-ui-date-time-picker.element.js';
import UmbPropertyEditorUIDateOnlyPickerElement from './date-only-picker/property-editor-ui-date-only-picker.element.js';
import UmbPropertyEditorUITimeOnlyPickerElement from './time-only-picker/property-editor-ui-time-only-picker.element.js';
import UmbPropertyEditorUIDateTimeWithTimeZonePickerElement from './date-time-with-time-zone-picker/property-editor-ui-date-time-with-time-zone-picker.element.js';

describe('UmbPropertyEditorUIDateTimePickerElement', () => {
	let element: UmbPropertyEditorUIDateTimePickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-date-time-picker></umb-property-editor-ui-date-time-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDateTimePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});

describe('UmbPropertyEditorUIDateTimeWithTimeZonePickerElement', () => {
	let element: UmbPropertyEditorUIDateTimeWithTimeZonePickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-date-time-with-time-zone-picker></umb-property-editor-ui-date-time-with-time-zone-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDateTimeWithTimeZonePickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});


describe('UmbPropertyEditorUIDateOnlyPickerElement', () => {
	let element: UmbPropertyEditorUIDateOnlyPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-date-only-picker></umb-property-editor-ui-date-only-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDateOnlyPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});

describe('UmbPropertyEditorUITimeOnlyPickerElement', () => {
	let element: UmbPropertyEditorUITimeOnlyPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-time-only-picker></umb-property-editor-ui-time-only-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUITimeOnlyPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
