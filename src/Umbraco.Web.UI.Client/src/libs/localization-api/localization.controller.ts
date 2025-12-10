/*
This module is a modified copy of the original Shoelace localize package: https://github.com/shoelace-style/localize

The original license is included below.

Copyright (c) 2020 A Beautiful Site, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
import type {
	UmbLocalizationSet,
	FunctionParams,
	UmbLocalizationSetBase,
	UmbLocalizationSetKey,
} from './localization.manager.js';
import { umbLocalizationManager } from './localization.manager.js';
import type { LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbController, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const LocalizationControllerAlias = Symbol();
/**
 * The UmbLocalizationController enables localization for your element.
 * @see UmbLocalizeElement
 * @example
 * ```ts
 * import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
 *
 * \@customElement('my-element')
 * export class MyElement extends LitElement {
 *   private localize = new UmbLocalizationController(this);
 *
 *   render() {
 *     return html`<p>${this.localize.term('general_close')}</p>`;
 *   }
 * }
 * ```
 */
export class UmbLocalizationController<LocalizationSetType extends UmbLocalizationSetBase = UmbLocalizationSet>
	implements UmbController
{
	#host;
	#hostEl?: HTMLElement & Partial<Pick<LitElement, 'requestUpdate'>>;
	readonly controllerAlias = LocalizationControllerAlias;
	#usedKeys = new Array<UmbLocalizationSetKey>();

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.#hostEl = host.getHostElement() as HTMLElement;
		this.#host.addUmbController(this);
	}

	hostConnected(): void {
		umbLocalizationManager.appendConsumer(this);
	}

	hostDisconnected(): void {
		umbLocalizationManager.removeConsumer(this);
	}

	destroy(): void {
		this.#host?.removeUmbController(this);
		this.#hostEl = undefined as any;
		this.#usedKeys.length = 0;
	}

	documentUpdate() {
		this.#hostEl?.requestUpdate?.();
	}

	keysChanged(changedKeys: Set<UmbLocalizationSetKey>) {
		const hasOneOfTheseKeys = this.#usedKeys.find((key) => changedKeys.has(key));

		if (hasOneOfTheseKeys) {
			this.#hostEl?.requestUpdate?.();
		}
	}

	/**
	 * Gets the host element's directionality as determined by the `dir` attribute. The return value is transformed to
	 * lowercase.
	 * @returns {string} - the directionality.
	 */
	dir() {
		return `${this.#hostEl?.dir || umbLocalizationManager.documentDirection}`.toLowerCase();
	}

	/**
	 * Gets the host element's language as determined by the `lang` attribute. The return value is transformed to
	 * lowercase.
	 * @returns {string} - the language code.
	 */
	lang() {
		return `${this.#hostEl?.lang || umbLocalizationManager.documentLanguage}`.toLowerCase();
	}

	#getLocalizationData(lang: string) {
		const locale = new Intl.Locale(lang);
		const language = locale?.language.toLowerCase();
		const region = locale?.region?.toLowerCase() ?? '';
		// TODO: Its a bit of a tight coupling here, as this code relies on localizations begin a map. We should abstract that away. [NL]
		// TODO: Currently we don't check if the `lang` is available. We should do that. We could maybe checking if the `lang` is in the `languages` map. [NL]?
		const primary = umbLocalizationManager.localizations.get(`${language}-${region}`) as LocalizationSetType;
		const secondary = umbLocalizationManager.localizations.get(language) as LocalizationSetType;

		return { locale, language, region, primary, secondary };
	}

	/**
	 * Looks up a localization entry for the given key.
	 * Searches in order: primary (regional) → secondary (language) → fallback (en).
	 * Also tracks the key usage for reactive updates.
	 * @param {string} key - the localization key to look up.
	 * @returns {any} - the localization entry (string or function), or null if not found.
	 */
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	#lookupTerm<K extends keyof LocalizationSetType>(key: K): any {
		if (!this.#usedKeys.includes(key)) {
			this.#usedKeys.push(key);
		}

		const { primary, secondary } = this.#getLocalizationData(this.lang());

		// Look for a matching term using regionCode, code, then the fallback
		if (primary?.[key]) {
			return primary[key];
		} else if (secondary?.[key]) {
			return secondary[key];
		} else if (umbLocalizationManager.fallback?.[key]) {
			return umbLocalizationManager.fallback[key];
		}

		return null;
	}

	/**
	 * Processes a localization entry (string or function) with the provided arguments.
	 * @param {any} term - the localization entry to process.
	 * @param {unknown[]} args - the arguments to apply to the term.
	 * @returns {string} - the processed term as a string.
	 */
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	#processTerm(term: any, args: unknown[]): string {
		if (typeof term === 'function') {
			return term(...args) as string;
		}

		if (typeof term === 'string') {
			if (args.length) {
				// Replace placeholders of format "%index%" and "{index}" with provided values
				return term.replace(/(%(\d+)%|\{(\d+)\})/g, (match, _p1, p2, p3): string => {
					const index = p2 || p3;
					return typeof args[index] !== 'undefined' ? String(args[index]) : match;
				});
			}
		}

		return String(term);
	}

	/**
	 * Outputs a translated term.
	 * @param {string} key - the localization key, the indicator of what localization entry you want to retrieve.
	 * @param {unknown[]} args - the arguments to parse for this localization entry.
	 * @returns {string} - the translated term as a string.
	 * @example
	 * Retrieving a term without any arguments:
	 * ```ts
	 * this.localize.term('area_term');
	 * ```
	 * Retrieving a term with arguments:
	 * ```ts
	 * this.localize.term('general_greeting', ['John']);
	 * ```
	 */
	term<K extends keyof LocalizationSetType>(key: K, ...args: FunctionParams<LocalizationSetType[K]>): string {
		const term = this.#lookupTerm(key);

		if (term === null) {
			return String(key);
		}

		return this.#processTerm(term, args);
	}

	/**
	 * Returns the localized term for the given key, or the default value if not found.
	 * This method follows the same resolution order as term() (primary → secondary → fallback),
	 * but returns the provided defaultValue instead of the key when no translation is found.
	 * @param {string} key - the localization key, the indicator of what localization entry you want to retrieve.
	 * @param {string | null} defaultValue - the value to return if the key is not found in any localization set.
	 * @param {unknown[]} args - the arguments to parse for this localization entry.
	 * @returns {string | null} - the translated term or the default value.
	 * @example
	 * Retrieving a term with fallback:
	 * ```ts
	 * this.localize.termOrDefault('general_close', 'X');
	 * ```
	 * Retrieving a term with fallback and arguments:
	 * ```ts
	 * this.localize.termOrDefault('general_greeting', 'Hello!', userName);
	 * ```
	 * Retrieving a term with null as fallback:
	 * ```ts
	 * this.localize.termOrDefault('general_close', null);
	 * ```
	 */
	termOrDefault<K extends keyof LocalizationSetType, D extends string | null>(
		key: K,
		defaultValue: D,
		...args: FunctionParams<LocalizationSetType[K]>
	): string | D {
		const term = this.#lookupTerm(key);

		if (term === null) {
			return defaultValue;
		}

		return this.#processTerm(term, args);
	}

	/**
	 * Outputs a localized date in the specified format.
	 * @param {Date} dateToFormat - the date to format.
	 * @param {Intl.DateTimeFormatOptions} options - the options to use when formatting the date.
	 * @returns {string}
	 */
	date(dateToFormat: Date | string, options?: Intl.DateTimeFormatOptions): string {
		dateToFormat = new Date(dateToFormat);
		return new Intl.DateTimeFormat(this.lang(), options).format(dateToFormat);
	}

	/**
	 * Outputs a localized number in the specified format.
	 * @param {number | string} numberToFormat - the number or string to format.
	 * @param {Intl.NumberFormatOptions} options - the options to use when formatting the number.
	 * @returns {string} - the formatted number.
	 */
	number(numberToFormat: number | string, options?: Intl.NumberFormatOptions): string {
		numberToFormat = Number(numberToFormat);
		return isNaN(numberToFormat) ? '' : new Intl.NumberFormat(this.lang(), options).format(numberToFormat);
	}

	/**
	 * Outputs a localized time in relative format.
	 * @example "in 2 days"
	 * @param {number} value - the value to format.
	 * @param {Intl.RelativeTimeFormatUnit} unit - the unit of time to format.
	 * @param {Intl.RelativeTimeFormatOptions} options - the options to use when formatting the time.
	 * @returns {string} - the formatted time.
	 */
	relativeTime(value: number, unit: Intl.RelativeTimeFormatUnit, options?: Intl.RelativeTimeFormatOptions): string {
		return new Intl.RelativeTimeFormat(this.lang(), options).format(value, unit);
	}

	/**
	 * Outputs a localized compounded time in a duration format.
	 * @example "2 days, 3 hours and 5 minutes"
	 * @param {Date} fromDate - the date to compare from.
	 * @param {Date} toDate - the date to compare to, usually the current date (default: current date).
	 * @param {object} options - the options to use when formatting the time.
	 * @returns {string} - the formatted time, example: "2 days, 3 hours, 5 minutes"
	 */
	duration(fromDate: Date | string, toDate?: Date | string, options?: any): string {
		const d1 = new Date(fromDate);
		const d2 = new Date(toDate ?? Date.now());
		const diff = Math.abs(d1.getTime() - d2.getTime());
		const diffInSecs = Math.abs(Math.floor(diff / 1000));

		if (false === 'DurationFormat' in Intl) {
			return `${diffInSecs} seconds`;
		}

		const diffInDays = Math.floor(diff / (1000 * 60 * 60 * 24));
		const restDiffInHours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
		const restDiffInMins = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
		const restDiffInSecs = Math.floor((diff % (1000 * 60)) / 1000);

		const formatOptions = {
			style: 'long',
			...options,
		};

		// TODO: This is a hack to get around the fact that the DurationFormat is not yet available in the TypeScript typings. [JOV]
		const formatter = new (Intl as any).DurationFormat(this.lang(), formatOptions);

		if (diffInDays === 0 && restDiffInHours === 0 && restDiffInMins === 0) {
			return formatter.format({ seconds: diffInSecs });
		}

		return formatter.format({
			days: diffInDays,
			hours: restDiffInHours,
			minutes: restDiffInMins,
			seconds: restDiffInSecs,
		});
	}

	/**
	 * Outputs a localized list of values in the specified format.
	 * @example "one, two, and three"
	 * @param {Iterable<string>} values - the values to format.
	 * @param {Intl.ListFormatOptions} options - the options to use when formatting the list.
	 * @returns {string} - the formatted list.
	 */
	list(values: Iterable<string>, options?: Intl.ListFormatOptions): string {
		return new Intl.ListFormat(this.lang(), options).format(values);
	}

	/**
	 * Translates a string containing one or more terms. The terms should be prefixed with a `#` character.
	 * If the term is found in the localization set, it will be replaced with the localized term.
	 * If the term is not found, the original term will be returned.
	 * @param {string | undefined} text The text to translate.
	 * @param {unknown[]} args The arguments to parse for this localization entry.
	 * @returns {string} The translated text.
	 */
	string(text: string | undefined, ...args: unknown[]): string {
		if (typeof text !== 'string') {
			return '';
		}

		// find all words starting with #
		const regex = /#\w+/g;

		const localizedText = text.replace(regex, (match: string) => {
			const key = match.slice(1) as keyof LocalizationSetType;

			const term = this.#lookupTerm(key);

			// we didn't find a localized string, so we return the original string with the #
			if (term === null) {
				return match;
			}

			return this.#processTerm(term, args);
		});

		return localizedText;
	}
}
