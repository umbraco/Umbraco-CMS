import { UmbInputNumberRangeElement } from './input-number-range.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbPropertyEditorUINumberRangeElement', () => {
	let element: UmbInputNumberRangeElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-number-range></umb-input-number-range> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputNumberRangeElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	it('treats a zero bound as a set value', () => {
		element.minValue = 0;
		element.maxValue = 5;
		expect(element.value).to.equal('0,5');
	});

	it('supports negative bounds', () => {
		element.minValue = -5;
		element.maxValue = 5;
		expect(element.value).to.equal('-5,5');
	});

	it('parses decimal values from the string value', () => {
		element.value = '1.5,3.5';
		expect(element.minValue).to.equal(1.5);
		expect(element.maxValue).to.equal(3.5);
	});

	it('is undefined when both bounds are unset', () => {
		element.minValue = undefined;
		element.maxValue = undefined;
		expect(element.value).to.equal(undefined);
	});
});
