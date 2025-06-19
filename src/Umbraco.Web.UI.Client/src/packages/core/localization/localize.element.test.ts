import { UmbLocalizeElement } from './localize.element.js';
import { aTimeout, elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbLocalizationRegistry } from './registry/localization.registry.js';
import type { ManifestLocalization } from './extensions/localization.extension.js';

const english: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en',
	name: 'Test English',
	weight: 100,
	meta: {
		culture: 'en',
		localizations: {
			general: {
				close: 'Close',
				logout: 'Log out',
				numUsersSelected: (count: number) => {
					if (count === 0) return 'No users selected';
					if (count === 1) return 'One user selected';
					return `${count} users selected`;
				},
				moreThanOneArgument: (arg1: string, arg2: string) => {
					return `${arg1} ${arg2}`;
				},
			},
		},
	},
};

const englishUs: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en-us',
	name: 'Test English (US)',
	weight: 100,
	meta: {
		culture: 'en-us',
		localizations: {
			general: {
				close: 'Close US',
				overridden: 'Overridden',
			},
		},
	},
};

// This is a factory function that returns the localization object.
const asyncFactory = async (localizations: Record<string, any>, delay: number) => {
	await aTimeout(delay); // Simulate async loading
	return {
		// Simulate a JS module that exports a localization object.
		default: localizations,
	};
};

// This is an async localization that overrides the previous one.
const englishAsyncOverride: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en.async-override',
	name: 'Test English Async Override',
	weight: -100,
	meta: {
		culture: 'en-us',
	},
	js: () =>
		asyncFactory(
			{
				general: {
					close: 'Close Async',
					overridden: 'Overridden Async',
				},
			},
			100,
		),
};

// This is another async localization that loads later than the previous one and overrides it because of a lower weight.
const english2AsyncOverride: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en.async-override-2',
	name: 'Test English Async Override 2',
	weight: -200,
	meta: {
		culture: 'en-us',
	},
	js: () =>
		asyncFactory(
			{
				general: {
					close: 'Another Async Close',
				},
			},
			200, // This will load after the first async override
			// so it should override the close translation.
			// The overridden translation should not be overridden.
		),
};

const danish: ManifestLocalization = {
	type: 'localization',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da',
		localizations: {
			general: {
				close: 'Luk',
			},
		},
	},
};

describe('umb-localize', () => {
	let element: UmbLocalizeElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-localize>Fallback value</umb-localize>`);
	});

	it('should be defined', () => {
		expect(element).to.be.instanceOf(UmbLocalizeElement);
	});

	describe('localization', () => {
		umbExtensionsRegistry.registerMany([english, englishUs, danish]);

		beforeEach(async () => {
			umbLocalizationRegistry.loadLanguage(english.meta.culture);
			element = await fixture(html`<umb-localize key="general_close">Fallback value</umb-localize>`);
		});

		it('should have a localize controller', () => {
			expect(element.localize).to.be.instanceOf(UmbLocalizationController);
		});

		it('should localize a key', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');
		});

		it('should localize a key with arguments', async () => {
			element.key = 'general_numUsersSelected';
			element.args = [0];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('No users selected');

			element.args = [1];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('One user selected');

			element.args = [2];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('2 users selected');
		});

		it('should localize a key with multiple arguments', async () => {
			element.key = 'general_moreThanOneArgument';
			element.args = ['Hello', 'World'];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Hello World');
		});

		it('should localize a key with multiple arguments as encoded HTML', async () => {
			element.key = 'general_moreThanOneArgument';
			element.args = ['<strong>Hello</strong>', '<em>World</em>'];
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('&lt;strong&gt;Hello&lt;/strong&gt; &lt;em&gt;World&lt;/em&gt;');
		});

		it('should localize a key with args as an attribute', async () => {
			element.key = 'general_moreThanOneArgument';
			element.setAttribute('args', '["Hello","World"]');
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Hello World');
		});

		it('should change the value if a new key is set', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			element.key = 'general_logout';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Log out');
		});

		it('should change the value if the language is changed', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			// Change to Danish
			umbLocalizationRegistry.loadLanguage(danish.meta.culture);
			await aTimeout(0);
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Luk');
		});

		it('should fall back to the fallback language if the key is not found', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			// Change to US English
			umbLocalizationRegistry.loadLanguage(englishUs.meta.culture);
			await aTimeout(0);
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Close US');

			element.key = 'general_overridden';
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Overridden');

			element.key = 'general_logout';
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain('Log out');
		});

		it('should accept a lazy loaded localization', async () => {
			umbExtensionsRegistry.registerMany([englishAsyncOverride, english2AsyncOverride]);
			umbLocalizationRegistry.loadLanguage(englishAsyncOverride.meta.culture);
			await aTimeout(200); // Wait for the async override to load

			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain(
				'Another Async Close',
				'(async) Should have overridden the close (from first language)',
			);

			element.key = 'general_overridden';
			await elementUpdated(element);
			expect(element.shadowRoot?.innerHTML).to.contain(
				'Overridden Async',
				'(async) Should not have overridden the overridden (from first language)',
			);
		});

		it('should use the slot if translation is not found', async () => {
			element.key = 'non-existing-key';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('<slot></slot>');
		});

		it('should toggle a data attribute', async () => {
			element.key = 'non-existing-key';
			await elementUpdated(element);

			expect(element.getAttribute('data-localize-missing')).to.equal('non-existing-key');

			element.key = 'general_close';
			await elementUpdated(element);

			expect(element.hasAttribute('data-localize-missing')).to.equal(false);
		});

		it('should use the key if debug is enabled and translation is not found', async () => {
			element.key = 'non-existing-key';
			element.debug = true;
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('non-existing-key');
		});
	});
});
