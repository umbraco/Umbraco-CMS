import { expect, aTimeout } from '@open-wc/testing';
import { firstValueFrom } from 'rxjs';
import { UmbLocalizationContext } from './localization.context.js';
import { UmbTranslationRegistry } from './registry/translation.registry.js';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';

//#region Translations
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

const englishOverride = {
	type: 'translations',
	alias: 'test.en.override',
	name: 'Test English',
	meta: {
		culture: 'en',
		translations: {
			general: {
				close: 'Close 2',
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
//#endregion

describe('Localization', () => {
	let registry: UmbTranslationRegistry;
	let extensionRegistry: UmbExtensionRegistry<any>;

	beforeEach(() => {
		extensionRegistry = new UmbExtensionRegistry();
		registry = new UmbTranslationRegistry(extensionRegistry);
		extensionRegistry.register(english);
		extensionRegistry.register(danish);
		registry.register(english.meta.culture, english.meta.culture);
	});

	describe('UmbTranslationRegistry', () => {
		it('should register and get translation', (done) => {
			registry.translations.subscribe((translations) => {
				expect(translations.get('general_close')).to.equal('Close');
				done();
			});
		});
	});

	describe('UmbLocalizationContext', () => {
		let context: UmbLocalizationContext;

		beforeEach(async () => {
			context = new UmbLocalizationContext(extensionRegistry);
			context.setLanguage(english.meta.culture, english.meta.culture);
		});

		describe('localize', () => {
			it('should return a value', async () => {
				const value = await firstValueFrom(context.localize('general_close'));
				expect(value).to.equal('Close');
			});

			it('should return fallback if key is not found', async () => {
				const value = await firstValueFrom(context.localize('general_not_found', 'Not found'));
				expect(value).to.equal('Not found');
			});

			it('should return an empty string if fallback is not provided', async () => {
				const value = await firstValueFrom(context.localize('general_not_found'));
				expect(value).to.equal('');
			});

			it('should return a new value if a key is overridden', async () => {
				const value = await firstValueFrom(context.localize('general_close'));
				expect(value).to.equal('Close');

				extensionRegistry.register(englishOverride);

				const value2 = await firstValueFrom(context.localize('general_close'));
				expect(value2).to.equal('Close 2');
			});

			it('should return a new value if the language is changed', async () => {
				const value = await firstValueFrom(context.localize('general_close'));
				expect(value).to.equal('Close');

				context.setLanguage(danish.meta.culture, english.meta.culture);

				await aTimeout(0);

				const value2 = await firstValueFrom(context.localize('general_close'));
				expect(value2).to.equal('Luk');
			});

			it('should use fallback language if key is not found', async () => {
				const value = await firstValueFrom(context.localize('general_logout'));
				expect(value).to.equal('Log out');

				context.setLanguage(danish.meta.culture, english.meta.culture);

				await aTimeout(0);

				const value2 = await firstValueFrom(context.localize('general_logout'));
				expect(value2).to.equal('Log out');
			});
		});

		describe('localizeMany', () => {
			it('should return values', async () => {
				const values = await firstValueFrom(context.localizeMany(['general_close', 'general_logout']));
				expect(values[0]).to.equal('Close');
				expect(values[1]).to.equal('Log out');
			});

			it('should return empty values if keys are not found', async () => {
				const values = await firstValueFrom(context.localizeMany(['general_close', 'general_not_found']));
				expect(values[0]).to.equal('Close');
				expect(values[1]).to.equal('');
			});

			it('should update values if a key is overridden', async () => {
				const values = await firstValueFrom(context.localizeMany(['general_close', 'general_logout']));
				expect(values[0]).to.equal('Close');
				expect(values[1]).to.equal('Log out');

				extensionRegistry.register(englishOverride);

				const values2 = await firstValueFrom(context.localizeMany(['general_close', 'general_logout']));
				expect(values2[0]).to.equal('Close 2');
				expect(values2[1]).to.equal('Log out');
			});

			it('should return new values if a language is changed', async () => {
				const values = await firstValueFrom(context.localizeMany(['general_close', 'general_logout']));
				expect(values[0]).to.equal('Close');
				expect(values[1]).to.equal('Log out');

				context.setLanguage(danish.meta.culture, english.meta.culture);

				await aTimeout(0);

				const values2 = await firstValueFrom(context.localizeMany(['general_close', 'general_logout']));
				expect(values2[0]).to.equal('Luk');
				expect(values2[1]).to.equal('Log out'); // This key does not exist in the danish translation so should use 'en' fallback.
			});
		});

		it('should emit new values in same subscription', async () => {
			const values: string[][] = [];

			context.localizeMany(['general_close', 'general_logout']).subscribe((value) => {
				values.push(value);
			});

			// Let the subscription run (values are available statically)
			await aTimeout(0);

			expect(values[0][0]).to.equal('Close');
			expect(values[0][1]).to.equal('Log out');

			// it should return new values if a key is overridden
			extensionRegistry.register(englishOverride);

			// Let the subscription run again
			await aTimeout(0);

			expect(values[1][0]).to.equal('Close 2');
			expect(values[1][1]).to.equal('Log out');
		});
	});
});
