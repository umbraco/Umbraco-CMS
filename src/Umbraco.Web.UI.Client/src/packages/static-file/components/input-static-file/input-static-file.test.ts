import { expect, fixture, html } from '@open-wc/testing';
import { UmbInputStaticFileElement } from './input-static-file.element.js';
import { defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbInputStaticFileElement', () => {
	let element: UmbInputStaticFileElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-static-file></umb-input-static-file> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputStaticFileElement);
	});

	if ((window as any).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('Public API', () => {
		describe('properties', () => {
			it('has a selectedPaths property', () => {
				expect(element).to.have.property('selectedPaths').to.be.an.instanceOf(Array);
			});

			it('has a value property', () => {
				expect(element).to.have.property('value').that.is.a('string');
			});
		});
	});
});
