import { UmbLocalizationRegistry } from './localization.registry.js';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestLocalization } from '../extensions/localization.extension.js';

//#region Localizations
const englishUk: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en',
	name: 'Test English (UK)',
	weight: 100,
	meta: {
		culture: 'en',
		direction: 'ltr',
		localizations: {
			general: {
				color: 'Colour',
			},
		},
	},
};

const english: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en-us',
	name: 'Test English (US)',
	weight: 100,
	meta: {
		culture: 'en-us',
		direction: 'ltr',
		localizations: {
			general: {
				color: 'Color',
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
	weight: 0,
	meta: {
		culture: 'en-us',
		localizations: {
			general: {
				close: 'Close 2',
				overridden: 'Overridden',
			},
		},
	},
};

// This is a factory function that returns the localization object.
const englishAsyncFactory = async () => {
	await aTimeout(100); // Simulate async loading
	return {
		// Simulate a JS module that exports a localization object.
		default: {
			general: {
				close: 'Close Async',
				overridden: 'Overridden Async',
			},
		},
	};
};

const englishAsyncOverride: ManifestLocalization = {
	type: 'localization',
	alias: 'test.en.async-override',
	name: 'Test English Async Override',
	weight: -100,
	meta: {
		culture: 'en-us',
	},
	js: englishAsyncFactory,
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
	umbExtensionsRegistry.registerMany([englishUk, english, danish, danishRegional]);

	let registry: UmbLocalizationRegistry;

	beforeEach(async () => {
		registry = new UmbLocalizationRegistry(umbExtensionsRegistry);
		registry.loadLanguage(english.meta.culture);
		await aTimeout(0);
	});

	afterEach(() => {
		registry.localizations.clear();
		registry.destroy();
	});

	it('should register into the localization manager', async () => {
		expect(registry.localizations.size).to.equal(2, 'Should have registered the 2 original iso codes (en, en-us)');

		// Register an additional language to test the registry.
		registry.loadLanguage(danish.meta.culture);
		await aTimeout(0);
		expect(registry.localizations.size).to.equal(3, 'Should have registered the 3rd language (da)');
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

		const current = registry.localizations.get(englishOverride.meta.culture);
		expect(current).to.have.property(
			'general_close',
			'Close 2',
			'Should have overridden the close (from first language)',
		);
		expect(current).to.have.property('general_logout', 'Log out', 'Should not have overridden the logout');

		umbExtensionsRegistry.unregister(englishOverride.alias);
	});

	it('should load translations based on weight (lowest weight overrides)', async () => {
		// set weight to 200, so it will not override the existing translation
		const englishOverrideLowWeight = { ...englishOverride, weight: 200 } satisfies ManifestLocalization;
		umbExtensionsRegistry.register(englishOverrideLowWeight);
		await aTimeout(0);

		let current = registry.localizations.get(englishOverrideLowWeight.meta.culture);
		expect(current).to.have.property(
			'general_close',
			'Close',
			'Should not have overridden the close (from first language)',
		);
		expect(current).to.have.property('general_overridden', 'Overridden', 'Should be able to register its own keys');

		// Now register a new async override with a lower weight
		umbExtensionsRegistry.register(englishAsyncOverride);
		await aTimeout(200); // Wait for the async override to load
		current = registry.localizations.get(englishOverrideLowWeight.meta.culture);
		expect(current).to.have.property(
			'general_close',
			'Close Async',
			'(async) Should have overridden the close (from first language)',
		);
		expect(current).to.have.property(
			'general_overridden',
			'Overridden Async',
			'(async) Should have overridden the overridden',
		);

		umbExtensionsRegistry.unregister(englishOverrideLowWeight.alias);
		umbExtensionsRegistry.unregister(englishAsyncOverride.alias);
	});

	it('should be able to switch to the fallback language', async () => {
		// Verify that the document language and direction is set correctly to the default language
		expect(document.documentElement.lang).to.equal(english.meta.culture);
		expect(document.documentElement.dir).to.equal(english.meta.direction);

		// Switch to the fallback language, which is the UK version of English
		registry.loadLanguage('en');
		await aTimeout(0);

		expect(document.documentElement.lang).to.equal('en');
		expect(document.documentElement.dir).to.equal('ltr');

		const current = registry.localizations.get(englishUk.meta.culture);
		expect(current).to.have.property('general_color', 'Colour');

		// And switch back again
		registry.loadLanguage('en-us');
		await aTimeout(0);

		expect(document.documentElement.lang).to.equal('en-us');
		expect(document.documentElement.dir).to.equal('ltr');

		const newCurrent = registry.localizations.get(english.meta.culture);
		expect(newCurrent).to.have.property('general_color', 'Color');
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
