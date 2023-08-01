import { Translation, registerTranslation } from '../manager.js';
import { hasDefaultExport, loadExtension } from '@umbraco-cms/backoffice/extension-api';
import { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { Subject, combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

export type UmbTranslationDictionary = Record<string, unknown>;

export class UmbTranslationRegistry {
	#registry;
	#currentLanguage = new Subject<string>();

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		this.#registry = extensionRegistry;

		combineLatest([this.#currentLanguage, this.#registry.extensionsOfType('translations')]).subscribe(
			async ([userCulture, extensions]) => {
				await Promise.all(
					extensions
						.filter((x) => x.meta.culture.toLowerCase() === userCulture)
						.map(async (extension) => {
							const innerDictionary: UmbTranslationDictionary = {};

							// If extension contains a dictionary, add it to the inner dictionary.
							if (extension.meta.translations) {
								for (const [dictionaryName, dictionary] of Object.entries(extension.meta.translations)) {
									this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
								}
							}

							// If extension contains a js file, load it and add the default dictionary to the inner dictionary.
							const loadedExtension = await loadExtension(extension);

							if (loadedExtension && hasDefaultExport(loadedExtension)) {
								for (const [dictionaryName, dictionary] of Object.entries(loadedExtension.default)) {
									this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
								}
							}

							// Notify subscribers that the inner dictionary has changed.
							const translation: Translation = {
								$code: userCulture,
								$dir: extension.meta.direction ?? 'ltr',
								...innerDictionary,
							};
							registerTranslation(translation);

							// Set the document language and direction.
							document.documentElement.lang = translation.$code;
							document.documentElement.dir = translation.$dir;
						})
				);
			}
		);
	}

	loadLanguage(userCulture: string) {
		this.#currentLanguage.next(userCulture.toLowerCase());
	}

	#addOrUpdateDictionary(
		innerDictionary: UmbTranslationDictionary,
		dictionaryName: string,
		dictionary: UmbTranslationDictionary
	) {
		for (const [key, value] of Object.entries(dictionary)) {
			innerDictionary[`${dictionaryName}_${key}`] = value;
		}
	}
}
