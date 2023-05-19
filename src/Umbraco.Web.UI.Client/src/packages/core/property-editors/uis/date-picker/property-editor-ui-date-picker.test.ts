import { expect, fixture, html } from '@open-wc/testing';
import { UmbDateInputElement } from '../../../components/date-input/date-input.element';
import { UmbPropertyEditorUIDatePickerElement } from './property-editor-ui-date-picker.element';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/data-type';

describe('UmbPropertyEditorUIDatePickerElement', () => {
	let element: UmbPropertyEditorUIDatePickerElement;
	let inputElement: UmbDateInputElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-date-picker></umb-property-editor-ui-date-picker> `);
		inputElement = element.shadowRoot?.querySelector('umb-date-input') as UmbDateInputElement;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIDatePickerElement);
	});

	it('should have an input element', () => {
		expect(inputElement).to.exist;
	});

	it('should show a datetime-local input by default', () => {
		expect(inputElement.type).to.equal('datetime-local');
	});

	it('should show a type=date field if the format only contains a date', async () => {
		element.config = new UmbDataTypePropertyCollection([{ alias: 'format', value: 'YYYY-MM-dd' }]);
		await element.updateComplete;
		expect(inputElement.type).to.equal('date');
	});

	it('should show a type=time field if the format only contains a time', async () => {
		element.config = new UmbDataTypePropertyCollection([{ alias: 'format', value: 'HH:mm' }]);
		await element.updateComplete;
		expect(inputElement.type).to.equal('time');
	});

	it('passes the a11y audit', async () => {
		await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
	});
});
