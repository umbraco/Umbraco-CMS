import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLocalizeDateElement } from './localize-date.element.js';
import { umbLocalizationRegistry } from './registry/localization.registry.js';
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
		umbLocalizationRegistry.loadLanguage('en');

		it('should localize a date', () => {
			expect(element.shadowRoot?.textContent).to.equal('01/01/2020');
		});

		it('should localize a date with options', async () => {
			element.options = { dateStyle: 'full' };
			await element.updateComplete;

			expect(element.shadowRoot?.textContent).to.equal('Wednesday, 1 January 2020');
		});

		it('should set a title', async () => {
			await aTimeout(0);
			expect(element.title).to.equal('2 years ago');
		});

		it('should not set a title', async () => {
			element.skipDuration = true;
			element.title = 'Another title';
			await element.updateComplete;
			expect(element.title).to.equal('Another title');
		});
	});
});

describe('umb-localize-date with an explicitly requested region', () => {
	// `en` is corrected to en-GB for formatting, so this suite switches the active language to a
	// specific region, which must be left alone. Hence its own hooks rather than the shared ones.
	const englishUs = {
		type: 'localization',
		alias: 'test.en-us',
		name: 'Test English (US)',
		meta: { culture: 'en-us' },
	};

	before(async () => {
		umbExtensionsRegistry.register(englishUs);
		umbLocalizationRegistry.loadLanguage('en-US');
		await aTimeout(0);
	});

	after(async () => {
		umbExtensionsRegistry.unregister(englishUs.alias);
		umbLocalizationRegistry.loadLanguage('en');
		await aTimeout(0);
	});

	it('formats the date with the requested region', async () => {
		const element: UmbLocalizeDateElement = await fixture(
			html`<umb-localize-date .date=${new Date('2020-09-01T00:00:00')}></umb-localize-date>`,
		);

		expect(element.shadowRoot?.textContent).to.equal('9/1/2020');
	});
});
