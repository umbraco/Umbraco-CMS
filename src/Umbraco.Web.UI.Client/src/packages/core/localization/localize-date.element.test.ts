import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLocalizeDateElement } from './localize-date.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';

const english = {
	type: 'localization',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en',
		localizations: {
			general: {
				duration: () => {
					return '2 years ago'; // This is a simplified version of the actual implementation
				},
			},
		},
	},
};

describe('umb-localize-date', () => {
	let date: Date;
	let element: UmbLocalizeDateElement;

	beforeEach(async () => {
		date = new Date('2020-01-01T00:00:00');
		element = await fixture(html`<umb-localize-date .date=${date}>Fallback value</umb-localize-date>`);
	});

	it('should be defined', () => {
		expect(element).to.be.instanceOf(UmbLocalizeDateElement);
	});

	describe('localization', () => {
		umbExtensionsRegistry.register(english);

		it('should localize a date', () => {
			expect(element.shadowRoot?.textContent).to.equal('1/1/2020');
		});

		it('should localize a date with options', async () => {
			element.options = { dateStyle: 'full' };
			await element.updateComplete;

			expect(element.shadowRoot?.textContent).to.equal('Wednesday, January 1, 2020');
		});

		it('should set a title', async () => {
			await aTimeout(0);
			expect(element.title).to.equal('2 years ago');
		});
	});
});
