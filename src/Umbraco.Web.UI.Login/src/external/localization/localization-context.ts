import { firstValueFrom, ReplaySubject } from 'rxjs';

class UmbLocalizationContext {
    #innerDictionary = new ReplaySubject<Map<string, string>>(1);

    constructor() {
      this.#initLocalizedResources();
    }

    async #initLocalizedResources(): Promise<void> {
      const innerDictionary = new Map<string, string>();
      /*const response = await fetch(this.#apiBasePath);

      if (!response.ok) {
          throw new Error(`Failed to fetch localized resources: ${response.status} ${response.statusText}`);
      }

      // Resources are returned as a dictionary of dictionaries and we need to flatten it to a single dictionary.
      // The keys need to be prefixed with the dictionary name and an underscore.
      const resources = await response.json();
      for (const [dictionaryName, dictionary] of Object.entries(resources)) {
          for (const [key, value] of Object.entries(dictionary as Record<string, string>)) {
              innerDictionary.set(`${dictionaryName}_${key}`, value);
          }
      }*/

      this.#innerDictionary.next(innerDictionary);
    }

    /**
     * Localize a key.
     * If the key is not found, the fallback is returned.
     * If the fallback is not provided, the key is returned.
     * @param key The key to localize. The key is case sensitive.
     * @param tokens The tokens to replace in the localized text (default: undefined).
     * @param fallback The fallback text to use if the key is not found (default: undefined).
     */
    async localize(key: string, tokens?: any[], fallback?: string): Promise<string> {
        return this._lookup(key, tokens, fallback);
    }

    /**
     * Replaces tokens in a string with the values provided.
     * @param text
     * @param tokens
     */
    tokenReplace(text: string, tokens?: any[]) {
        if (tokens) {
            for (let i = 0; i < tokens.length; i++) {
                text = text.replace("%" + i + "%", tokens[i]);
            }
        }

        return text;
    }

    private async _lookup(alias: string, tokens?: any[], fallbackValue?: string) {
        const dictionary = await firstValueFrom(this.#innerDictionary);

        //strip the key identifier if its there
        if (alias && alias[0] === "@") {
            alias = alias.substring(1);
        }

        let underscoreIndex = alias.indexOf("_");

        //if no area specified, add general_
        if (alias && underscoreIndex < 0) {
            alias = "general_" + alias;
        }

        const valueEntry = dictionary.get(alias);

        if (typeof valueEntry !== 'undefined') {
            return this.tokenReplace(valueEntry, tokens);
        }

        return fallbackValue ?? `[${alias}]`;
    }
}

export const umbLocalizationContext = new UmbLocalizationContext();
export type { UmbLocalizationContext };
