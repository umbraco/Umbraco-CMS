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
		expect(element.value).to.be.undefined;
	});

	it('keeps the separator when only the minimum is set', () => {
		element.minValue = 5;
		element.maxValue = undefined;
		expect(element.value).to.equal('5,');
	});

	it('keeps the separator when only the maximum is set', () => {
		element.minValue = undefined;
		element.maxValue = 10;
		expect(element.value).to.equal(',10');
	});

	it('parses a value with only the minimum set', () => {
		element.value = '5,';
		expect(element.minValue).to.equal(5);
		expect(element.maxValue).to.be.undefined;
	});

	it('parses a value with only the maximum set', () => {
		element.value = ',10';
		expect(element.minValue).to.be.undefined;
		expect(element.maxValue).to.equal(10);
	});

	it('is invalid when the minimum exceeds the maximum', () => {
		element.minValue = 5;
		element.maxValue = 2;
		element.checkValidity();
		expect(element.validity.valid).to.be.false;
	});

	it('is valid when only the minimum is set', () => {
		element.minValue = 5;
		element.maxValue = undefined;
		element.checkValidity();
		expect(element.validity.valid).to.be.true;
	});

	it('is valid when only the maximum is set', () => {
		element.minValue = undefined;
		element.maxValue = 2;
		element.checkValidity();
		expect(element.validity.valid).to.be.true;
	});
});
