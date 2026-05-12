/* eslint-disable @typescript-eslint/naming-convention */
/*
This module is a modified copy of the original Shoelace localize package: https://github.com/shoelace-style/localize

The original license is included below.

Copyright (c) 2020 A Beautiful Site, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
import type { UmbLocalizationController } from './localization.controller.js';
import type { UmbLocalizationEntry } from './types/localization.js';

export type FunctionParams<T> = T extends (...args: infer U) => string ? U : [];

export interface UmbLocalizationSetBase {
	$code: string; // e.g. en, en-GB
	$dir: 'ltr' | 'rtl';
}

export type UmbLocalizationSetKey = string | number | symbol;

export interface UmbLocalizationSet extends UmbLocalizationSetBase {
	[key: UmbLocalizationSetKey]: UmbLocalizationEntry;
}

export const UMB_DEFAULT_LOCALIZATION_CULTURE = 'en';

export class UmbLocalizationManager {
	connectedControllers = new Set<UmbLocalizationController<UmbLocalizationSetBase>>();

	#changedKeys: Set<UmbLocalizationSetKey> = new Set();
	#requestUpdateChangedKeysId?: number = undefined;

	localizations: Map<string, UmbLocalizationSetBase> = new Map();
	documentDirection: 'ltr' | 'rtl' = (document.documentElement.dir as 'ltr' | 'rtl') || 'ltr';
	documentLanguage = document.documentElement.lang || navigator.language;

	get fallback(): UmbLocalizationSet | undefined {
		return this.localizations.get(UMB_DEFAULT_LOCALIZATION_CULTURE) as UmbLocalizationSet;
	}

	appendConsumer(consumer: UmbLocalizationController<UmbLocalizationSetBase>) {
		if (this.connectedControllers.has(consumer)) return;
		this.connectedControllers.add(consumer);
	}
	removeConsumer(consumer: UmbLocalizationController<UmbLocalizationSetBase>) {
		this.connectedControllers.delete(consumer);
	}

	/**
	 * Registers one or more translations
	 * @param t
	 */
	registerLocalization(t: UmbLocalizationSetBase) {
		const code = t.$code.toLowerCase();

		if (this.localizations.has(code)) {
			// Merge translations that share the same language code
			this.localizations.set(code, { ...this.localizations.get(code), ...t });
		} else {
			this.localizations.set(code, t);
		}

		// Declare what keys have been changed:
		const keys = Object.keys(t);
		for (const key of keys) {
			this.#changedKeys.add(key);
		}
		this.#requestChangedKeysUpdate();
	}
	#registerLocalizationBind = this.registerLocalization.bind(this);

	registerManyLocalizations(translations: Array<UmbLocalizationSetBase>) {
		translations.map(this.#registerLocalizationBind);
	}

	/**
	 * Sets the active language and direction and notifies all connected controllers.
	 * This is the single channel through which the manager learns of a language change;
	 * callers should not write to `document.documentElement.lang` for this purpose.
	 * @param {string} language The language code to set as active.
	 * @param {'ltr' | 'rtl'} [direction] The direction associated with the language.
	 */
	setActiveLanguage(language: string, direction: 'ltr' | 'rtl' = 'ltr', options?: { silent?: boolean }) {
		const newLang = language || UMB_DEFAULT_LOCALIZATION_CULTURE;
		const changed = this.documentLanguage !== newLang || this.documentDirection !== direction;

		this.documentLanguage = newLang;
		this.documentDirection = direction;

		// When `silent` is set, callers want to update the fields without notifying consumers.
		// This is used by the registry to set the active language synchronously when it changes,
		// so controllers can pick up the new language on their next render, while deferring the
		// `documentUpdate` notification until translations have actually loaded.
		if (options?.silent || !changed) return;

		this.connectedControllers.forEach((ctrl) => {
			ctrl.documentUpdate();
		});

		if (this.#requestUpdateChangedKeysId) {
			cancelAnimationFrame(this.#requestUpdateChangedKeysId);
			this.#requestUpdateChangedKeysId = undefined;
		}
		this.#changedKeys.clear();
	}

	/**
	 * Explicitly notify all connected controllers that the active language or its translations
	 * have changed. Use this after registering or loading a new set of translations, when the
	 * `documentLanguage`/`documentDirection` fields may not have changed but the underlying
	 * dictionaries did. Tests around the unrelated key-change debounce still rely on the
	 * documentUpdate path being the canonical "force-rerender" channel.
	 */
	notifyLanguageChanged() {
		this.connectedControllers.forEach((ctrl) => {
			ctrl.documentUpdate();
		});

		if (this.#requestUpdateChangedKeysId) {
			cancelAnimationFrame(this.#requestUpdateChangedKeysId);
			this.#requestUpdateChangedKeysId = undefined;
		}
		this.#changedKeys.clear();
	}

	#updateChangedKeys = () => {
		this.#requestUpdateChangedKeysId = undefined;

		this.connectedControllers.forEach((ctrl) => {
			ctrl.keysChanged(this.#changedKeys);
		});

		this.#changedKeys.clear();
	};

	/**
	 * Request an update of all consumers of the keys defined in #changedKeys.
	 * This waits one frame, which ensures that multiple changes are collected into one.
	 */
	#requestChangedKeysUpdate() {
		if (this.#requestUpdateChangedKeysId) return;
		this.#requestUpdateChangedKeysId = requestAnimationFrame(this.#updateChangedKeys);
	}
}

export const umbLocalizationManager = new UmbLocalizationManager();
