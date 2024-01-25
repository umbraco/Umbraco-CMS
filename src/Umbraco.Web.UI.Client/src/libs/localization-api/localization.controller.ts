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
 * The UmbLocalizeController enables localization for your element.
 *
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
		this.#host.addController(this);
	}

	hostConnected(): void {
		umbLocalizationManager.appendConsumer(this);
	}

	hostDisconnected(): void {
		umbLocalizationManager.removeConsumer(this);
	}

	destroy(): void {
		this.#host.removeController(this);
		this.#hostEl = undefined as any;
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
	 */
	dir() {
		return `${this.#hostEl?.dir || umbLocalizationManager.documentDirection}`.toLowerCase();
	}

	/**
	 * Gets the host element's language as determined by the `lang` attribute. The return value is transformed to
	 * lowercase.
	 */
	lang() {
		return `${this.#hostEl?.lang || umbLocalizationManager.documentLanguage}`.toLowerCase();
	}

	private getLocalizationData(lang: string) {
		const locale = new Intl.Locale(lang);
		const language = locale?.language.toLowerCase();
		const region = locale?.region?.toLowerCase() ?? '';
		const primary = umbLocalizationManager.localizations.get(`${language}-${region}`) as LocalizationSetType;
		const secondary = umbLocalizationManager.localizations.get(language) as LocalizationSetType;

		return { locale, language, region, primary, secondary };
	}

	/** Outputs a translated term. */
	term<K extends keyof LocalizationSetType>(key: K, ...args: FunctionParams<LocalizationSetType[K]>): string {
		if (!this.#usedKeys.includes(key)) {
			this.#usedKeys.push(key);
		}

		const { primary, secondary } = this.getLocalizationData(this.lang());
		let term: any;

		// Look for a matching term using regionCode, code, then the fallback
		if (primary && primary[key]) {
			term = primary[key];
		} else if (secondary && secondary[key]) {
			term = secondary[key];
		} else if (umbLocalizationManager.fallback && umbLocalizationManager.fallback[key]) {
			term = umbLocalizationManager.fallback[key];
		} else {
			return String(key);
		}

		if (typeof term === 'function') {
			return term(...args) as string;
		}

		if (typeof term === 'string') {
			if (args.length > 0) {
				// Replace placeholders of format "%index%" and "{index}" with provided values
				term = term.replace(/(%(\d+)%|\{(\d+)\})/g, (match, _p1, p2, p3): string => {
					const index = p2 || p3;
					return String(args[index] || match);
				});
			}
		}

		return term;
	}

	/** Outputs a localized date in the specified format. */
	date(dateToFormat: Date | string, options?: Intl.DateTimeFormatOptions): string {
		dateToFormat = new Date(dateToFormat);
		return new Intl.DateTimeFormat(this.lang(), options).format(dateToFormat);
	}

	/** Outputs a localized number in the specified format. */
	number(numberToFormat: number | string, options?: Intl.NumberFormatOptions): string {
		numberToFormat = Number(numberToFormat);
		return isNaN(numberToFormat) ? '' : new Intl.NumberFormat(this.lang(), options).format(numberToFormat);
	}

	/** Outputs a localized time in relative format. */
	relativeTime(value: number, unit: Intl.RelativeTimeFormatUnit, options?: Intl.RelativeTimeFormatOptions): string {
		return new Intl.RelativeTimeFormat(this.lang(), options).format(value, unit);
	}
}
