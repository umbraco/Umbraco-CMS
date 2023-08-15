import {
	UmbTranslationsDictionary,
	UmbTranslationsFlatDictionary,
	TranslationSet,
	registerTranslation,
	translations,
} from '@umbraco-cms/backoffice/localization-api';
import { hasDefaultExport, loadExtension } from '@umbraco-cms/backoffice/extension-api';
import { UmbBackofficeExtensionRegistry, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { BehaviorSubject, Subject, combineLatest, map, distinctUntilChanged, filter, startWith } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbTranslationRegistry {
	/**
	 * Get the current registered translations.
	 */
	get translations() {
		return translations;
	}

	get isDefaultLoaded() {
		return this.#isDefaultLoaded.asObservable();
	}

	#currentLanguage = new Subject<string>();
	#isDefaultLoaded = new BehaviorSubject(false);

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		const currentLanguage$ = this.#currentLanguage.pipe(
			startWith(document.documentElement.lang || 'en-us'),
			map((x) => x.toLowerCase()),
			distinctUntilChanged()
		);

		const currentExtensions$ = extensionRegistry.extensionsOfType('translations').pipe(
			filter((x) => x.length > 0),
			distinctUntilChanged((prev, curr) => prev.length === curr.length && prev.every((x) => curr.includes(x)))
		);

		combineLatest([currentLanguage$, currentExtensions$]).subscribe(async ([userCulture, extensions]) => {
			const locale = new Intl.Locale(userCulture);
			const translations = await Promise.all(
				extensions
					.filter(
						(x) =>
							x.meta.culture.toLowerCase() === locale.baseName.toLowerCase() ||
							x.meta.culture.toLowerCase() === locale.language.toLowerCase()
					)
					.map(async (extension) => {
						const innerDictionary: UmbTranslationsFlatDictionary = {};

						// If extension contains a dictionary, add it to the inner dictionary.
						if (extension.meta.translations) {
							for (const [dictionaryName, dictionary] of Object.entries(extension.meta.translations)) {
								this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
							}
						}

						// If extension contains a js file, load it and add the default dictionary to the inner dictionary.
						const loadedExtension = await loadExtension(extension);

						if (loadedExtension && hasDefaultExport<UmbTranslationsDictionary>(loadedExtension)) {
							for (const [dictionaryName, dictionary] of Object.entries(loadedExtension.default)) {
								this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
							}
						}

						// Notify subscribers that the inner dictionary has changed.
						return {
							$code: extension.meta.culture.toLowerCase(),
							$dir: extension.meta.direction ?? 'ltr',
							...innerDictionary,
						} satisfies TranslationSet;
					})
			);

			if (translations.length) {
				registerTranslation(...translations);

				// Set the document language
				const newLang = locale.baseName.toLowerCase();
				if (document.documentElement.lang.toLowerCase() !== newLang) {
					document.documentElement.lang = newLang;
				}

				// Set the document direction to the direction of the primary language
				const newDir = translations[0].$dir ?? 'ltr';
				if (document.documentElement.dir !== newDir) {
					document.documentElement.dir = newDir;
				}
			}

			if (!this.#isDefaultLoaded.value) {
				this.#isDefaultLoaded.next(true);
				this.#isDefaultLoaded.complete();
			}
		});
	}

	/**
	 * Load a language from the extension registry.
	 * @param locale The locale to load.
	 */
	loadLanguage(locale: string) {
		this.#currentLanguage.next(locale);
	}

	#addOrUpdateDictionary(
		innerDictionary: UmbTranslationsFlatDictionary,
		dictionaryName: string,
		dictionary: UmbTranslationsDictionary['value']
	) {
		for (const [key, value] of Object.entries(dictionary)) {
			innerDictionary[`${dictionaryName}_${key}`] = value;
		}
	}
}

export const umbTranslationRegistry = new UmbTranslationRegistry(umbExtensionsRegistry);
