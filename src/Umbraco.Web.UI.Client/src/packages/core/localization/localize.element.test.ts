import { elementUpdated, expect, fixture, html } from '@open-wc/testing';
import { UmbLocalizeElement } from './localize.element.js';

import '@umbraco-cms/backoffice/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTranslationRegistry } from '@umbraco-cms/backoffice/localization-api';

const english = {
	type: 'translations',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en',
		translations: {
			general: {
				close: 'Close',
				logout: 'Log out',
			},
		},
	},
};

const danish = {
	type: 'translations',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da',
		translations: {
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
		umbExtensionsRegistry.register(english);
		umbExtensionsRegistry.register(danish);
		const translationRegistry = new UmbTranslationRegistry(umbExtensionsRegistry);
		translationRegistry.loadLanguage(english.meta.culture);
		translationRegistry.loadLanguage(danish.meta.culture);

		beforeEach(async () => {
			element = await fixture(html`<umb-localize key="general_close"></umb-localize>`);
		});

		it('should localize a key', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');
		});

		it('should change the value if a new key is set', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			element.key = 'general_logout';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Log out');
		});

		it('should change the value if the language is changed', async () => {
			expect(element.shadowRoot?.innerHTML).to.contain('Close');

			element.lang = danish.meta.culture;
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('Luk');
		});

		it('should use the slot if translation is not found', async () => {
			element.key = 'non-existing-key';
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('<slot></slot>');

			// It should also have a data attribute to indicate that the key was not found:
			expect(element.getAttribute('data-localize-missing')).to.equal('non-existing-key');
		});

		it('should use the key if debug is enabled and translation is not found', async () => {
			element.key = 'non-existing-key';
			element.debug = true;
			await elementUpdated(element);

			expect(element.shadowRoot?.innerHTML).to.contain('non-existing-key');
		});
	});
});
