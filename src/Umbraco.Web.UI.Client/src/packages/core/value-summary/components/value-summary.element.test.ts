import './value-summary.element.js';
import { UmbValueSummaryElement } from './value-summary.element.js';
import { expect, fixture, html } from '@open-wc/testing';

describe('UmbValueSummaryElement', () => {
	let element: UmbValueSummaryElement;

	afterEach(() => {
		// fixture handles its own cleanup
	});

	it('should be defined', async () => {
		element = await fixture(html`<umb-value-summary></umb-value-summary>`);
		expect(element).to.be.instanceOf(UmbValueSummaryElement);
	});

	it('should accept valueType and value properties', async () => {
		element = await fixture(html`<umb-value-summary .valueType=${'test'} .value=${'hello'}></umb-value-summary>`);
		expect(element.valueType).to.equal('test');
		expect(element.value).to.equal('hello');
	});

	it('should render nothing without valueType', async () => {
		element = await fixture(html`<umb-value-summary .value=${'hello'}></umb-value-summary>`);
		expect(element.shadowRoot!.innerHTML).to.not.contain('umb-extension-with-api-slot');
	});

	it('should render extension-with-api-slot when valueType is set', async () => {
		element = await fixture(
			html`<umb-value-summary .valueType=${'Umb.Test.Slot'} .value=${'hello'}></umb-value-summary>`,
		);
		const slot = element.shadowRoot!.querySelector('umb-extension-with-api-slot');
		expect(slot).to.not.be.null;
		expect(slot!.getAttribute('type')).to.equal('valueSummary');
	});
});
