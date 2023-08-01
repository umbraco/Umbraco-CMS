/*
This module is a modified copy of the original Shoelace localize package: https://github.com/shoelace-style/localize

The original license is included below.

Copyright (c) 2020 A Beautiful Site, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
import {
	DefaultTranslation,
	FunctionParams,
	Translation,
	connectedElements,
	documentDirection,
	documentLanguage,
	fallback,
	translations,
} from './manager.js';
import { UmbController, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

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
export class UmbLocalizeController<UserTranslation extends Translation = DefaultTranslation> implements UmbController {
	host;
	controllerAlias = 'localize';

	constructor(host: UmbControllerHostElement) {
		this.host = host;
		this.host.addController(this);
	}

	hostConnected(): void {
		if (connectedElements.has(this.host)) {
			return;
		}

		connectedElements.add(this.host);
	}

	hostDisconnected(): void {
		connectedElements.delete(this.host);
	}

	destroy(): void {
		connectedElements.delete(this.host);
		this.host.removeController(this);
	}

	/**
	 * Gets the host element's directionality as determined by the `dir` attribute. The return value is transformed to
	 * lowercase.
	 */
	dir() {
		return `${this.host.dir || documentDirection}`.toLowerCase();
	}

	/**
	 * Gets the host element's language as determined by the `lang` attribute. The return value is transformed to
	 * lowercase.
	 */
	lang() {
		return `${this.host.lang || documentLanguage}`.toLowerCase();
	}

	private getTranslationData(lang: string) {
		const locale = new Intl.Locale(lang);
		const language = locale?.language.toLowerCase();
		const region = locale?.region?.toLowerCase() ?? '';
		const primary = <UserTranslation>translations.get(`${language}-${region}`);
		const secondary = <UserTranslation>translations.get(language);

		return { locale, language, region, primary, secondary };
	}

	/** Outputs a translated term. */
	term<K extends keyof UserTranslation>(key: K, ...args: FunctionParams<UserTranslation[K]>): string {
		const { primary, secondary } = this.getTranslationData(this.lang());
		let term: any;

		// Look for a matching term using regionCode, code, then the fallback
		if (primary && primary[key]) {
			term = primary[key];
		} else if (secondary && secondary[key]) {
			term = secondary[key];
		} else if (fallback && fallback[key as keyof Translation]) {
			term = fallback[key as keyof Translation];
		} else {
			return String(key);
		}

		if (typeof term === 'function') {
			return term(...args) as string;
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
