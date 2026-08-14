import { UmbIconElement } from './icon.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
describe('UmbIconElement', () => {
	let element: UmbIconElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-icon></umb-icon> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbIconElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('color handling', () => {
		it('does not set --uui-icon-color when no color is provided', () => {
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('');
		});

		it('sets --uui-icon-color to a raw color value', async () => {
			element.color = '#ff0000';
			await element.updateComplete;
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('#ff0000');
		});

		it('resolves a color alias to the matching CSS variable', async () => {
			element.color = 'color-red';
			await element.updateComplete;
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('var(--uui-palette-maroon-flush)');
		});

		it('resolves a color suffix from the name property', async () => {
			element.name = 'icon-heart color-red';
			await element.updateComplete;
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('var(--uui-palette-maroon-flush)');
		});

		it('prefers the color property over the name suffix', async () => {
			element.name = 'icon-heart color-red';
			element.color = 'color-green';
			await element.updateComplete;
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('var(--uui-palette-jungle-green)');
		});

		it('removes --uui-icon-color when color is cleared', async () => {
			element.color = 'color-red';
			await element.updateComplete;
			element.color = '';
			await element.updateComplete;
			expect(element.style.getPropertyValue('--uui-icon-color')).to.equal('');
		});
	});
});
