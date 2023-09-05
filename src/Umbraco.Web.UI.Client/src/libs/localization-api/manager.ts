/*
This module is a modified copy of the original Shoelace localize package: https://github.com/shoelace-style/localize

The original license is included below.

Copyright (c) 2020 A Beautiful Site, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
import type { UmbLocalizationEntry } from './types/localization.js';
import type { LitElement } from '@umbraco-cms/backoffice/external/lit';

export type FunctionParams<T> = T extends (...args: infer U) => string ? U : [];

export interface TranslationSet {
	$code: string; // e.g. en, en-GB
	$dir: 'ltr' | 'rtl';
}

export interface DefaultTranslationSet extends TranslationSet {
	[key: string]: UmbLocalizationEntry;
}

export const connectedElements = new Set<HTMLElement>();
const documentElementObserver = new MutationObserver(update);
export const translations: Map<string, TranslationSet> = new Map();
export let documentDirection = document.documentElement.dir || 'ltr';
export let documentLanguage = document.documentElement.lang || navigator.language;
export let fallback: TranslationSet;

// Watch for changes on <html lang>
documentElementObserver.observe(document.documentElement, {
	attributes: true,
	attributeFilter: ['dir', 'lang'],
});

/** Registers one or more translations */
export function registerTranslation(...translation: TranslationSet[]) {
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
			// TODO: We might want to implement a specific Umbraco method for informing about this. and then make the default UmbLitElement call requestUpdate..? Cause then others can implement their own solution?
			(el as LitElement).requestUpdate();
		}
	});
}
