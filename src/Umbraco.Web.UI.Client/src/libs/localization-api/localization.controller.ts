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
import { escapeHTML } from '@umbraco-cms/backoffice/utils';

const LocalizationControllerAlias = Symbol();
/**
 * The UmbLocalizeController enables localization for your element.
 * @see UmbLocalizeElement
 * @example
 * ```ts
 * import { UmbLocalizeController } from '@umbraco-cms/backoffice/localization-api';
 *
 * \@customElement('my-element')
 * export class MyElement extends LitElement {
 *   private localize = new UmbLocalizeController(this);
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

	private getLocalizationData(lang: string) {
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
	 * Outputs a translated term.
	 * @param {string} key - the localization key, the indicator of what localization entry you want to retrieve.
	 * @param {...any} args - the arguments to parse for this localization entry.
	 * @returns {string} - the translated term as a string.
	 */
	term<K extends keyof LocalizationSetType>(key: K, ...args: FunctionParams<LocalizationSetType[K]>): string {
		if (!this.#usedKeys.includes(key)) {
			this.#usedKeys.push(key);
		}

		const { primary, secondary } = this.getLocalizationData(this.lang());

		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		let term: any;

		// Look for a matching term using regionCode, code, then the fallback
		if (primary?.[key]) {
			term = primary[key];
		} else if (secondary?.[key]) {
			term = secondary[key];
		} else if (umbLocalizationManager.fallback?.[key]) {
			term = umbLocalizationManager.fallback[key];
		} else {
			return String(key);
		}

		// As translated texts can contain HTML, we will need to render with unsafeHTML.
		// But arguments can come from user input, so they should be escaped.
		const sanitizedArgs = args.map((a) => escapeHTML(a));

		if (typeof term === 'function') {
			return term(...sanitizedArgs) as string;
		}

		if (typeof term === 'string') {
			if (sanitizedArgs.length) {
				// Replace placeholders of format "%index%" and "{index}" with provided values
				term = term.replace(/(%(\d+)%|\{(\d+)\})/g, (match, _p1, p2, p3): string => {
					const index = p2 || p3;
					return typeof sanitizedArgs[index] !== 'undefined' ? String(sanitizedArgs[index]) : match;
				});
			}
		}

		return term;
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
	 * @param {number} value - the value to format.
	 * @param {Intl.RelativeTimeFormatUnit} unit - the unit of time to format.
	 * @param {Intl.RelativeTimeFormatOptions} options - the options to use when formatting the time.
	 * @returns {string} - the formatted time.
	 */
	relativeTime(value: number, unit: Intl.RelativeTimeFormatUnit, options?: Intl.RelativeTimeFormatOptions): string {
		return new Intl.RelativeTimeFormat(this.lang(), options).format(value, unit);
	}

	/**
	 * Translates a string containing one or more terms. The terms should be prefixed with a `#` character.
	 * If the term is found in the localization set, it will be replaced with the localized term.
	 * If the term is not found, the original term will be returned.
	 * @param {string} text The text to translate.
	 * @returns {string} The translated text.
	 */
	string(text: unknown): string {
		if (typeof text !== 'string') {
			return '';
		}

		// find all words starting with #
		const regex = /#\w+/g;

		const localizedText = text.replace(regex, (match: string) => {
			const key = match.slice(1);
			// TODO: find solution to pass dynamic string to term
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			const localized = this.term(key);
			// we didn't find a localized string, so we return the original string with the #
			return localized === key ? match : localized;
		});

		return localizedText;
	}
}
