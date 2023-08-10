import { aTimeout, expect } from '@open-wc/testing';
import { UmbTranslationRegistry } from './translation.registry.js';
import { ManifestTranslations, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

//#region Translations
const english: ManifestTranslations = {
	type: 'translations',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		direction: 'ltr',
		translations: {
			general: {
				close: 'Close',
				logout: 'Log out',
				withInlineToken: '{0} {1}',
				withInlineTokenLegacy: '%0% %1%',
				numUsersSelected: (count: number) => {
					if (count === 0) return 'No users selected';
					if (count === 1) return 'One user selected';
					return `${count} users selected`;
				},
			},
		},
	},
};

const englishOverride: ManifestTranslations = {
	type: 'translations',
	alias: 'test.en.override',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		translations: {
			general: {
				close: 'Close 2',
			},
		},
	},
};

const danish: ManifestTranslations = {
	type: 'translations',
	alias: 'test.da',
	name: 'Test Danish',
	meta: {
		culture: 'da',
		translations: {
			general: {
				close: 'Luk',
				notOnRegional: 'Not on regional',
			},
		},
	},
};

const danishRegional: ManifestTranslations = {
	type: 'translations',
	alias: 'test.da-DK',
	name: 'Test Danish (Denmark)',
	meta: {
		culture: 'da-dk',
		translations: {
			general: {
				close: 'Luk',
			},
		},
	},
};
//#endregion

describe('UmbLocalizeController', () => {
	umbExtensionsRegistry.register(english);
	umbExtensionsRegistry.register(danish);
	umbExtensionsRegistry.register(danishRegional);

	let registry: UmbTranslationRegistry;

	beforeEach(async () => {
		registry = new UmbTranslationRegistry(umbExtensionsRegistry);
		registry.loadLanguage(english.meta.culture);
		await aTimeout(0);
	});

	afterEach(() => {
		registry.translations.clear();
	});

	it('should set the document language and direction', async () => {
		expect(document.documentElement.lang).to.equal(english.meta.culture);
		expect(document.documentElement.dir).to.equal(english.meta.direction);
	});

	it('should load translations for the current language', async () => {
		expect(registry.translations.has(english.meta.culture)).to.be.true;

		const current = registry.translations.get(english.meta.culture);
		expect(current).to.have.property('general_close', 'Close'); // Also tests that the translation is flattened.
		expect(current).to.have.property('general_logout', 'Log out');
	});

	it('should override translations with extensions of the same culture', async () => {
		umbExtensionsRegistry.register(englishOverride);

		await aTimeout(0);

		const current = registry.translations.get(english.meta.culture);
		expect(current).to.have.property('general_close', 'Close 2');
		expect(current).to.have.property('general_logout', 'Log out');
	});

	it('should load a new language', async () => {
		registry.loadLanguage(danish.meta.culture);

		await aTimeout(0);

		// Check that the new language is loaded.
		expect(registry.translations.has(danish.meta.culture)).to.be.true;

		// Check that the new language has the correct translations.
		const current = registry.translations.get(danish.meta.culture);
		expect(current).to.have.property('general_close', 'Luk');
	});

	it('should load translations for the current language and regional', async () => {
		// Load the regional language.
		registry.loadLanguage(danishRegional.meta.culture);
		await aTimeout(0);

		// Check that both the regional and the base language is loaded.
		expect(registry.translations.has(danishRegional.meta.culture), 'expected "da-dk" to be present').to.be.true;
		expect(registry.translations.has(danish.meta.culture), 'expected "da" to be present').to.be.true;
	});
});
