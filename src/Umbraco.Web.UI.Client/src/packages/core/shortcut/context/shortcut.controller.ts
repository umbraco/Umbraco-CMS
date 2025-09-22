import type { UmbShortcut } from '../types.js';
import { UMB_SHORTCUT_CONTEXT } from './shortcut.context-token.js';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbPartialSome } from '@umbraco-cms/backoffice/utils';

type IncomingShortcutType = UmbPartialSome<UmbShortcut, 'unique' | 'weight' | 'modifier' | 'shift' | 'alt'>;

const IsMac = navigator.userAgent ? /Mac/i.test(navigator.userAgent) : navigator.platform.toUpperCase().includes('MAC');

export class UmbShortcutController extends UmbControllerBase {
	//
	#inUnprovidingState = false;

	#parent?: UmbShortcutController;

	readonly #shortcuts = new UmbArrayState<UmbShortcut>([], (x) => x.unique);
	public readonly all = this.#shortcuts.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);

		this.#shortcuts.sortBy((a, b) => (b.weight || 0) - (a.weight || 0));
	}

	#providerCtrl?: UmbContextProviderController;
	#currentProvideHost?: UmbClassInterface;
	/**
	 * Provide this validation context to a specific controller host.
	 * This can be used to Host a validation context in a Workspace, but provide it on a certain scope, like a specific Workspace View.
	 * @param {UmbClassInterface} controllerHost - The controller host to provide this validation context to.
	 */
	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_SHORTCUT_CONTEXT, this as any);
	}

	unprovide(): void {
		if (this.#providerCtrl) {
			// We need to set this in Unprovide state, so this context can be provided again later.
			this.#inUnprovidingState = true;
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
			this.#inUnprovidingState = false;
			this.#currentProvideHost = undefined;
		}
	}

	inherit(): void {
		this.consumeContext(UMB_SHORTCUT_CONTEXT, (parent) => {
			this.inheritFrom(parent);
		}).skipHost();
		// Notice skipHost ^^, this is because we do not want it to consume it self, as this would be a match for this consumption, instead we will look at the parent and above. [NL]
	}

	inheritFrom(parent: UmbShortcutController | undefined): void {
		if (this.#parent === parent) return;
		this.#parent = parent;
	}

	initiateChange() {
		this.#shortcuts.mute();
	}
	finishChange() {
		this.#shortcuts.unmute();
	}

	/**
	 * Add a new hint
	 * @param {IncomingShortcutType} shortcut - The hint to add
	 * @returns {UmbShortcut['unique']} Unique value of the hint
	 */
	addOne(shortcut: IncomingShortcutType): string | symbol {
		const newShortcut = { ...shortcut } as unknown as UmbShortcut;
		newShortcut.unique ??= Symbol();
		newShortcut.weight ??= 0;
		newShortcut.modifier ??= false;
		newShortcut.shift ??= false;
		newShortcut.alt ??= false;
		this.#shortcuts.appendOne(newShortcut);
		return shortcut.unique!;
	}

	/**
	 * Add multiple rules
	 * @param {IncomingShortcutType[]} shortcuts - Array of hints to add
	 */
	add(shortcuts: IncomingShortcutType[]) {
		this.#shortcuts.mute();
		shortcuts.forEach((hint) => this.addOne(hint));
		this.#shortcuts.unmute();
	}

	/**
	 * Remove a hint
	 * @param {UmbShortcut['unique']} unique Unique value of the hint to remove
	 */
	removeOne(unique: UmbShortcut['unique']) {
		this.#shortcuts.removeOne(unique);
	}

	/**
	 * Remove multiple hints
	 * @param {UmbShortcut['unique'][]} uniques Array of unique values to remove
	 */
	remove(uniques: UmbShortcut['unique'][]) {
		this.#shortcuts.remove(uniques);
	}

	/**
	 * Check if a hint exists
	 * @param {UmbShortcut['unique']} unique Unique value of the hint to check
	 * @returns {boolean} True if the hint exists, false otherwise
	 */
	has(unique: UmbShortcut['unique']): boolean {
		return this.#shortcuts.getHasOne(unique);
	}

	/**
	 * Get all hints
	 * @returns {UmbShortcut[]} Array of hints
	 */
	getAll(): UmbShortcut[] {
		return this.#shortcuts.getValue();
	}

	/**
	 * Get all hints
	 * @param key
	 * @param modifier
	 * @param shift
	 * @param alt
	 * @returns {UmbShortcut[]} Array of hints
	 */
	findShortcut(key: string, modifier: boolean, shift: boolean = false, alt: boolean = false): UmbShortcut | undefined {
		const shortcuts = this.#shortcuts.getValue();
		for (const s of shortcuts) {
			if (s.key.toLowerCase() === key.toLowerCase() && s.modifier === modifier && s.shift === shift && s.alt === alt) {
				return s;
			}
		}

		return undefined;
	}

	/**
	 * Clear all hints
	 */
	clear(): void {
		this.#shortcuts.setValue([]);
	}

	activate() {
		window.addEventListener('keydown', this.#onKeyDown);
	}

	deactivate() {
		window.removeEventListener('keydown', this.#onKeyDown);
	}

	#onKeyDown = (e: KeyboardEvent) => {
		const keyDown = e.key.toLowerCase();
		const modifierDown = IsMac ? e.metaKey : e.ctrlKey;

		const shortcut = this.findShortcut(keyDown, modifierDown, e.shiftKey, e.altKey);
		if (shortcut) {
			e.preventDefault();
			shortcut.action();
		}
	};

	override destroy(): void {
		super.destroy();
		if (this.#inUnprovidingState === true) {
			// TODO: What is it i'm doing here, check if it actually makes sense, if so add a comment on why [NL]
			return;
		}
		this.unprovide();
		this.#parent = undefined;

		this.#shortcuts.destroy();
	}
}
