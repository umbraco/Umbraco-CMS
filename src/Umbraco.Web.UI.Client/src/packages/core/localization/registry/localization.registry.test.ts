import { UmbLocalizationRegistry } from './localization.registry.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestLocalization } from '../extensions/localization.extension.js';

//#region Localizations
const english: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		direction: 'ltr',
		localizations: {
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

const englishOverride: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en.override',
	name: 'Test English',
	meta: {
		culture: 'en-us',
		localizations: {
			general: {
				close: 'Close 2',
			},
		},
	},
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
				notOnRegional: 'Not on regional',
			},
		},
	},
};

const danishRegional: ManifestLocalization = {
	type: 'localization',
	alias: 'test.da-DK',
	name: 'Test Danish (Denmark)',
	meta: {
		culture: 'da-dk',
		localizations: {
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

	let registry: UmbLocalizationRegistry;

	beforeEach(async () => {
		registry = new UmbLocalizationRegistry(umbExtensionsRegistry);
		registry.loadLanguage(english.meta.culture);
		await aTimeout(0);
	});

	afterEach(() => {
		registry.localizations.clear();
	});

	it('should set the document language and direction', async () => {
		expect(document.documentElement.lang).to.equal(english.meta.culture);
		expect(document.documentElement.dir).to.equal(english.meta.direction);
	});

	it('should load translations for the current language', async () => {
		expect(registry.localizations.has(english.meta.culture)).to.be.true;

		const current = registry.localizations.get(english.meta.culture);
		expect(current).to.have.property('general_close', 'Close'); // Also tests that the translation is flattened.
		expect(current).to.have.property('general_logout', 'Log out');
	});

	it('should override translations with extensions of the same culture', async () => {
		umbExtensionsRegistry.register(englishOverride);

		await aTimeout(0);

		const current = registry.localizations.get(english.meta.culture);
		expect(current).to.have.property('general_close', 'Close 2');
		expect(current).to.have.property('general_logout', 'Log out');
	});

	it('should load a new language', async () => {
		registry.loadLanguage(danish.meta.culture);

		await aTimeout(0);

		// Check that the new language is loaded.
		expect(registry.localizations.has(danish.meta.culture)).to.be.true;

		// Check that the new language has the correct translations.
		const current = registry.localizations.get(danish.meta.culture);
		expect(current).to.have.property('general_close', 'Luk');
	});

	it('should load translations for the current language and regional', async () => {
		// Load the regional language.
		registry.loadLanguage(danishRegional.meta.culture);
		await aTimeout(0);

		// Check that both the regional and the base language is loaded.
		expect(registry.localizations.has(danishRegional.meta.culture), 'expected "da-dk" to be present').to.be.true;
		expect(registry.localizations.has(danish.meta.culture), 'expected "da" to be present').to.be.true;
	});
});
