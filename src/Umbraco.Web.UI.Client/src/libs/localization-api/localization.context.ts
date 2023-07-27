import { UmbTranslationRegistry } from './registry/translation.registry.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { combineLatest, distinctUntilChanged, type Observable, map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbLocalizationContext {
	#translationRegistry;

	constructor(umbExtensionRegistry: UmbBackofficeExtensionRegistry) {
		this.#translationRegistry = new UmbTranslationRegistry(umbExtensionRegistry);
	}

	get translations() {
		return this.#translationRegistry.translations;
	}

	/**
	 * Set a new language which will query the manifests for translations and register them.
	 * Eventually it will update all codes visible on the screen.
	 *
	 * @param languageIsoCode The language to use (example: 'en-us')
	 * @param fallbackLanguageIsoCode The fallback language to use (example: 'en-us', default: 'en-us')
	 */
	setLanguage(languageIsoCode: string, fallbackLanguageIsoCode?: string) {
		this.#translationRegistry.register(languageIsoCode, fallbackLanguageIsoCode);
	}

	/**
	 * Localize a key.
	 * If the key is not found, the fallback is returned.
	 * If the fallback is not provided, the key is returned.
	 *
	 * @param key The key to localize. The key is case sensitive.
	 * @param fallback The fallback text to use if the key is not found (default: undefined).
	 * @example localize('general_close').subscribe((value) => {
	 * 	console.log(value); // 'Close'
	 * });
	 */
	localize(key: string, fallback?: string): Observable<string> {
		return this.translations.pipe(
			map((dictionary) => {
				return dictionary.get(key) ?? fallback ?? '';
			})
		);
	}

	/**
	 * Localize many keys at once.
	 * If a key is not found, the key is returned.
	 *
	 * @description This method combines the results of multiple calls to localize.
	 * @param keys The keys to localize. The keys are case sensitive.
	 * @example localizeMany(['general_close', 'general_logout']).subscribe((values) => {
	 * 	console.log(values[0]); // 'Close'
	 * 	console.log(values[1]); // 'Log out'
	 * });
	 * @see localize
	 */
	localizeMany(keys: string[]) {
		return combineLatest(keys.map((key) => this.localize(key).pipe(distinctUntilChanged())));
	}
}

export const UMB_LOCALIZATION_CONTEXT = new UmbContextToken<UmbLocalizationContext>('UmbLocalizationContext');
