import { UmbTranslationRegistry } from './registry/translation.registry.js';
import { UMB_AUTH } from '@umbraco-cms/backoffice/auth';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { of, switchMap, type Observable, map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbLocalizationContext {
	#translationRegistry;

	constructor(host: UmbElement, umbExtensionRegistry: UmbBackofficeExtensionRegistry) {
		this.#translationRegistry = new UmbTranslationRegistry(umbExtensionRegistry);

		host.consumeContext(UMB_AUTH, (auth) => {
			host.observe(auth.currentUser, (user) => {
				if (user) {
					const languageIsoCode = user.languageIsoCode ?? 'en';
					this.#translationRegistry.register(languageIsoCode);
				}
			});
		});
	}

	get translations() {
		return this.#translationRegistry.translations;
	}

	/**
	 * Localize a key.
	 * If the key is not found, the fallback is returned.
	 * If the fallback is not provided, the key is returned.
	 * @param key The key to localize. The key is case sensitive.
	 * @param fallback The fallback text to use if the key is not found (default: undefined).
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
	 * @description This method combines the results of multiple calls to localize.
	 * @param keys
	 * @see localize
	 */
	localizeMany(keys: string[]): Observable<Record<string, string>> {
		return of(...keys).pipe(switchMap((key) => this.localize(key).pipe(map((value) => ({ [key]: value })))));
	}
}

export const UMB_LOCALIZATION_CONTEXT = new UmbContextToken<UmbLocalizationContext>('UmbLocalizationContext');
