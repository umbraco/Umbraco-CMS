import { LitElement } from '@umbraco-cms/backoffice/external/lit';

export type FunctionParams<T> = T extends (...args: infer U) => string ? U : [];

export interface Translation {
	$code: string; // e.g. en, en-GB
	$dir: 'ltr' | 'rtl';
}

export interface DefaultTranslation extends Translation {
	[key: string]: any;
}

export const connectedElements = new Set<HTMLElement>();
const documentElementObserver = new MutationObserver(update);
export const translations: Map<string, Translation> = new Map();
export let documentDirection = document.documentElement.dir || 'ltr';
export let documentLanguage = document.documentElement.lang || navigator.language;
export let fallback: Translation;

// Watch for changes on <html lang>
documentElementObserver.observe(document.documentElement, {
	attributes: true,
	attributeFilter: ['dir', 'lang'],
});

/** Registers one or more translations */
export function registerTranslation(...translation: Translation[]) {
	translation.map((t) => {
		const code = t.$code.toLowerCase();

		if (translations.has(code)) {
			// Merge translations that share the same language code
			translations.set(code, { ...translations.get(code), ...t });
		} else {
			translations.set(code, t);
		}

		// The first translation that's registered is the fallback
		if (!fallback) {
			fallback = t;
		}
	});

	update();
}

/** Updates all localized elements that are currently connected */
export function update() {
	documentDirection = document.documentElement.dir || 'ltr';
	documentLanguage = document.documentElement.lang || navigator.language;

	[...connectedElements.keys()].map((el) => {
		if (typeof (el as LitElement).requestUpdate === 'function') {
			(el as LitElement).requestUpdate();
		}
	});
}
