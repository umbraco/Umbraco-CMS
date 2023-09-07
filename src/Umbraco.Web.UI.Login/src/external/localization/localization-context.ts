import { firstValueFrom, ReplaySubject } from 'rxjs';

class UmbLocalizationContext {
	#init = false;
	readonly #apiBasePath = 'localizedtext';
	#innerDictionary = new ReplaySubject<Map<string, string>>(1);

	async #initLocalizedResources(): Promise<void> {
		if (this.#init) {
			return;
		}

		this.#init = true;

		const response = await fetch(this.#apiBasePath);

		if (!response.ok) {
			throw new Error(`Failed to fetch localized resources: ${response.status} ${response.statusText}`);
		}

		// Resources are returned as a dictionary of dictionaries and we need to flatten it to a single dictionary.
		// The keys need to be prefixed with the dictionary name and an underscore.
		const resources = await response.json();
		const innerDictionary = new Map<string, string>();
		for (const [dictionaryName, dictionary] of Object.entries(resources)) {
			for (const [key, value] of Object.entries(dictionary as Record<string, string>)) {
				innerDictionary.set(`${dictionaryName}_${key}`, value);
			}
		}

		this.#innerDictionary.next(innerDictionary);
	}

	/**
	 * Localize a key.
	 * If the key is not found, the fallback is returned.
	 * If the fallback is not provided, the key is returned.
	 * @param key The key to localize. The key is case sensitive.
	 * @param fallback The fallback text to use if the key is not found (default: undefined).
	 */
	async localize(key: string, fallback?: string): Promise<string> {
		this.#initLocalizedResources();

		return (await firstValueFrom(this.#innerDictionary)).get(key) ?? fallback ?? '';
	}

	/**
	 * Localize many keys.
	 * If a key is not found, the key is returned.
	 * @param keys
	 * @see localize
	 */
	async localizeMany(keys: string[]): Promise<string[]> {
		this.#initLocalizedResources();
		const innerDictionary = await firstValueFrom(this.#innerDictionary);
		return keys.map((key) => innerDictionary.get(key) ?? key ?? '');
	}
}

export const umbLocalizationContext = new UmbLocalizationContext();
export type { UmbLocalizationContext };
