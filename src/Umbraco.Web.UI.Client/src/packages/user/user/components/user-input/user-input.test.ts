import { UmbUserInputElement } from './user-input.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbContextConsumer } from '@umbraco-cms/backoffice/context-api';
import { UMB_PICKER_INPUT_CONTEXT } from '@umbraco-cms/backoffice/picker-input';

describe('UmbUserInputElement', () => {
	let element: UmbUserInputElement;

	const pickerModalData = async () => {
		const consumer = new UmbContextConsumer(element.shadowRoot!.querySelector('#btn-add')!, UMB_PICKER_INPUT_CONTEXT);
		consumer.hostConnected();
		const context = await consumer.asPromise();
		consumer.destroy();
		return context!.getModalData();
	};

	beforeEach(async () => {
		element = await fixture(html`<umb-user-input></umb-user-input>`);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbUserInputElement);
	});

	describe('pickable filters', () => {
		it('uses the browse pickable filter as the pickable filter of the picker', async () => {
			const browsePickableFilter = () => true;
			element.browsePickableFilter = browsePickableFilter;

			expect((await pickerModalData())?.pickableFilter).to.equal(browsePickableFilter);
		});

		it('returns the browse pickable filter it was given', () => {
			const browsePickableFilter = () => true;
			element.browsePickableFilter = browsePickableFilter;

			expect(element.browsePickableFilter).to.equal(browsePickableFilter);
		});
	});
});
