import type { UmbInputDateElement } from '@umbraco-cms/backoffice/components';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
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

	it('expands a date-only bound to span the whole day on a datetime-local field', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '2026-07-01' },
			{ alias: 'max', value: '2026-07-30' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01T00:00:00');
		expect(inputElement.max).to.equal('2026-07-30T23:59:59');
	});

	it('preserves an explicit time on a datetime-local bound', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '2026-07-01T09:30' },
			{ alias: 'max', value: '2026-07-30T17:45' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01T09:30:00');
		expect(inputElement.max).to.equal('2026-07-30T17:45:00');
	});

	it('parses a stored SQL-style datetime bound', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '2026-07-01 08:15:00' },
			{ alias: 'max', value: '2026-07-30 17:45:00' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01T08:15:00');
		expect(inputElement.max).to.equal('2026-07-30T17:45:00');
	});

	it('passes an unparseable bound through unchanged', async () => {
		element.config = new UmbPropertyEditorConfigCollection([{ alias: 'min', value: 'not-a-date' }]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('not-a-date');
	});

	it('leaves a bound unset when its config value is absent', async () => {
		element.config = new UmbPropertyEditorConfigCollection([{ alias: 'min', value: '2026-07-01' }]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01T00:00:00');
		expect(inputElement.max).to.be.undefined;
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

	it('applies min/max config to the date input, normalised for a datetime-local field', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '2026-07-01' },
			{ alias: 'max', value: '2026-07-30' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01T00:00:00');
		expect(inputElement.max).to.equal('2026-07-30T23:59:59');
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

	it('applies min/max config to the date input as date-only values', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '2026-07-01' },
			{ alias: 'max', value: '2026-07-30' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('2026-07-01');
		expect(inputElement.max).to.equal('2026-07-30');
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

	it('applies min/max config to the date input as time values', async () => {
		element.config = new UmbPropertyEditorConfigCollection([
			{ alias: 'min', value: '09:00:00' },
			{ alias: 'max', value: '17:30:00' },
		]);
		await element.updateComplete;
		const inputElement = element.shadowRoot!.querySelector('umb-input-date') as UmbInputDateElement;
		expect(inputElement.min).to.equal('09:00:00');
		expect(inputElement.max).to.equal('17:30:00');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
