import type { UmbPartialSome } from '../type/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbIncomingHintBase {
	unique?: string | symbol;
	text: string;
	weight?: number;
	color?: UUIInterfaceColor;
}

export interface UmbHint extends UmbIncomingHintBase {
	unique: string | symbol;
	weight: number;
}

export class UmbHintManager<
	HintType extends UmbHint = UmbHint,
	IncomingHintType extends UmbIncomingHintBase = UmbPartialSome<HintType, 'unique' | 'weight'>,
> extends UmbControllerBase {
	//
	#scaffold?: Partial<HintType>;

	protected readonly _hints = new UmbArrayState<HintType>([], (x) => x.unique);
	public readonly hints = this._hints.asObservable();
	public readonly firstHint = this._hints.asObservablePart((x) => x[0]);
	//public readonly hasHints = this._hints.asObservablePart((x) => x.length > 0);

	constructor(host: UmbControllerBase, args?: { scaffold?: Partial<HintType> }) {
		super(host);

		this.#scaffold = args?.scaffold;

		this._hints.sortBy((a, b) => (b.weight || 0) - (a.weight || 0));
	}

	asObservablePart<R>(fn: (hints: HintType[]) => R): Observable<R> {
		return this._hints.asObservablePart(fn);
	}

	#parent?: UmbHintManager;
	#parentHints: HintType[] = [];
	#localHints: HintType[] = [];
	bindWith(parent: UmbHintManager): void {
		if (this.#parent) {
			this.#parentHints = [];
			this.#localHints = [];
		}
		this.#parent = parent;
		this.observe(
			parent.hints,
			(hints) => {
				this._hints.mute();

				this.#parentHints = hints as unknown as HintType[];

				// Remove the local hints that does not exist in the parent anymore:
				const toRemove = this.#parentHints.filter((msg) => !hints.find((m) => m.unique === msg.unique));
				this._hints.remove(toRemove.map((msg) => msg.unique));
				this._hints.append(this.#parentHints);
				this.#localHints = this._hints.getValue();
				this._hints.unmute();
			},
			'observeParentHints',
		);
		this.observe(
			this.hints,
			(hints) => {
				if (!this.#parent) return;

				this.#parent!.initiateChange();

				// Remove the parent messages that does not exist locally anymore:
				const toRemove = this.#localHints.filter((locals) => !hints.find((m) => m.unique === locals.unique));
				this.#parent!.remove(toRemove.map((x) => x.unique));
				this.#parent!.add(hints);
				this.#parent!.finishChange();
			},
			'observeLocalHints',
		);
	}

	initiateChange() {
		this._hints.mute();
	}
	finishChange() {
		this._hints.unmute();
	}

	/**
	 * Add a new hint
	 * @param {HintType} hint - The hint to add
	 * @returns {HintType['unique']} Unique value of the hint
	 */
	addOne(hint: IncomingHintType): string | symbol {
		const newHint = { ...this.#scaffold, ...hint } as unknown as HintType;
		newHint.unique ??= Symbol();
		newHint.weight ??= 0;
		newHint.text ??= '!';
		this._hints.appendOne(newHint);
		return hint.unique!;
	}

	/**
	 * Add multiple rules
	 * @param {HintType[]} hints - Array of hints to add
	 */
	add(hints: IncomingHintType[]) {
		this._hints.mute();
		hints.forEach((hint) => this.addOne(hint));
		this._hints.unmute();
	}

	/**
	 * Remove a hint
	 * @param {HintType['unique']} unique Unique value of the hint to remove
	 */
	removeOne(unique: HintType['unique']) {
		this._hints.removeOne(unique);
	}

	/**
	 * Remove multiple hints
	 * @param {HintType['unique'][]} uniques Array of unique values to remove
	 */
	remove(uniques: HintType['unique'][]) {
		this._hints.remove(uniques);
	}

	/**
	 * Check if a hint exists
	 * @param {HintType['unique']} unique Unique value of the hint to check
	 * @returns {boolean} True if the hint exists, false otherwise
	 */
	has(unique: HintType['unique']): boolean {
		return this._hints.getHasOne(unique);
	}

	/**
	 * Get all hints
	 * @returns {HintType[]} Array of hints
	 */
	getAll(): HintType[] {
		return this._hints.getValue();
	}

	/**
	 * Clear all hints
	 */
	clear(): void {
		this._hints.setValue([]);
	}

	override destroy() {
		this._hints.destroy();
		super.destroy();
	}
}
