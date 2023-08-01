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
