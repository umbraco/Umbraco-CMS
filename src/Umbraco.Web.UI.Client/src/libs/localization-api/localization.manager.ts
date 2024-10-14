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
	#documentElementObserver: MutationObserver;

	#changedKeys: Set<UmbLocalizationSetKey> = new Set();
	#requestUpdateChangedKeysId?: number = undefined;

	localizations: Map<string, UmbLocalizationSetBase> = new Map();
	documentDirection = document.documentElement.dir || 'ltr';
	documentLanguage = document.documentElement.lang || navigator.language;

	get fallback(): UmbLocalizationSet | undefined {
		return this.localizations.get(UMB_DEFAULT_LOCALIZATION_CULTURE) as UmbLocalizationSet;
	}

	constructor() {
		this.#documentElementObserver = new MutationObserver(this.updateAll);
		this.#documentElementObserver.observe(document.documentElement, {
			attributes: true,
			attributeFilter: ['dir', 'lang'],
		});
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

	/** Updates all localized elements that are currently connected */
	updateAll = () => {
		const newDir = document.documentElement.dir || 'ltr';
		const newLang = document.documentElement.lang || navigator.language;

		if (this.documentDirection === newDir && this.documentLanguage === newLang) return;

		// The document direction or language did changed, so lets move on:
		this.documentDirection = newDir;
		this.documentLanguage = newLang;

		// Check if there was any changed.
		this.connectedControllers.forEach((ctrl) => {
			ctrl.documentUpdate();
		});

		if (this.#requestUpdateChangedKeysId) {
			cancelAnimationFrame(this.#requestUpdateChangedKeysId);
			this.#requestUpdateChangedKeysId = undefined;
		}
		this.#changedKeys.clear();
	};

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
