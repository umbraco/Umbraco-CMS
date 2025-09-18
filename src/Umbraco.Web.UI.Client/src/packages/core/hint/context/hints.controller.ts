import type { UmbPartialSome } from '../../utils/type/index.js';
import type { UmbHint, UmbIncomingHintBase } from '../types.js';
import { UMB_HINT_CONTEXT } from './hint.context-token.js';
import { UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, UmbObjectState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';

export interface UmbHintControllerArgs<HintType extends UmbHint = UmbHint> {
	viewAlias?: string | null;
	scaffold?: Partial<HintType>;
}

export class UmbHintController<
	HintType extends UmbHint = UmbHint,
	IncomingHintType extends UmbIncomingHintBase = UmbPartialSome<HintType, 'unique' | 'weight' | 'path'>,
> extends UmbControllerBase {
	//
	#viewAlias: string | null;
	getViewAlias(): string | null {
		return this.#viewAlias;
	}
	#pathFilter?: (path: Array<string>) => boolean;
	setPathFilter(filter: (path: Array<string>) => boolean) {
		this.#pathFilter = filter;
	}

	#scaffold = new UmbObjectState<Partial<HintType>>({});
	readonly scaffold = this.#scaffold.asObservable();
	#inUnprovidingState?: boolean;

	#parent?: UmbHintController;
	#parentHints?: Array<UmbHint>;

	readonly #hints = new UmbArrayState<HintType>([], (x) => x.unique);
	public readonly hints = this.#hints.asObservable();
	public readonly firstHint = this.#hints.asObservablePart((x) => x[0]);
	// Consider using weight to determine the visibility distance. [NL]
	//public readonly hasHints = this._hints.asObservablePart((x) => x.length > 0);

	updateScaffold(updates: Partial<HintType>) {
		this.#scaffold.update(updates);
	}
	getScaffold(): Partial<HintType> {
		return this.#scaffold.getValue();
	}

	constructor(host: UmbControllerHost, args?: UmbHintControllerArgs<HintType>) {
		super(host);

		this.#viewAlias = args?.viewAlias ?? null;
		if (args?.scaffold) {
			this.#scaffold.setValue(args?.scaffold);
		}

		this.#hints.sortBy((a, b) => (b.weight || 0) - (a.weight || 0));
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
		this.#providerCtrl = controllerHost.provideContext(UMB_HINT_CONTEXT, this as any);
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

	asObservablePart<R>(fn: (hints: HintType[]) => R): Observable<R> {
		return this.#hints.asObservablePart(fn);
	}

	descendingHints(viewAlias?: string | null): Observable<Array<UmbHint> | undefined> {
		if (viewAlias) {
			return this.#hints.asObservablePart((hints) => {
				return hints.filter((hint) => hint.path[0] === viewAlias);
			});
		} else {
			return this.hints;
		}
	}

	/**
	 * @internal
	 * @param {(path: Array<string>) => boolean} filter - A filter function to filter the hints by their path.
	 * @returns {Observable<Array<UmbHint> | undefined>} An observable of an array of hints that match the filter.
	 */
	// eslint-disable-next-line @typescript-eslint/naming-convention
	_internal_descendingHintsByFilter(filter: (path: Array<string>) => boolean): Observable<Array<UmbHint> | undefined> {
		return this.#hints.asObservablePart((hints) => {
			return hints.filter((hint) => filter(hint.path));
		});
	}

	inherit(): void {
		if (this.#viewAlias === null && this.#pathFilter === undefined) {
			throw new Error('A Hint Controller needs a view alias or pathFilter to be able to inherit from a parent.');
		}
		this.consumeContext(UMB_HINT_CONTEXT, (parent) => {
			this.inheritFrom(parent);
		}).skipHost();
		// Notice skipHost ^^, this is because we do not want it to consume it self, as this would be a match for this consumption, instead we will look at the parent and above. [NL]
	}

	inheritFrom(parent: UmbHintController | undefined): void {
		if (this.#parent === parent) return;
		if (this.#viewAlias === null && this.#pathFilter === undefined) {
			throw new Error('A Hint Controller needs a view alias or pathFilter to be able to inherit from a parent.');
		}
		this.#parent = parent;
		this.observe(this.#parent?.scaffold, (scaffold) => {
			if (scaffold) {
				this.#scaffold.update(scaffold as any);
			}
		});
		if (this.#viewAlias) {
			this.observe(parent?.descendingHints(this.#viewAlias), this.#receiveHints, 'observeParentHints');
		} else if (this.#pathFilter) {
			this.observe(
				parent?._internal_descendingHintsByFilter(this.#pathFilter),
				this.#receiveHints,
				'observeParentHints',
			);
		}
		this.observe(this.hints, this.#propagateHints, 'observeLocalMessages');
	}

	#receiveHints = (hints: UmbHint[] | undefined) => {
		if (!hints) {
			// Parent properly lost, so lets assume the parent hints are empty: [NL]
			hints = [];
		}
		this.initiateChange();
		if (this.#parentHints) {
			// Remove the local messages that does not exist in the parent anymore:
			const toRemove = this.#parentHints.filter((hint) => !hints.find((m) => m.unique === hint.unique));
			this.remove(toRemove.map((hint) => hint.unique));
		}
		this.#parentHints = hints;

		hints.forEach((hint) => {
			// Remove first entry of hint.path, if it matches viewAlias.
			if (this.#viewAlias && hint.path[0] === this.#viewAlias) {
				hint = { ...hint, path: hint.path.slice(1) };
			}
			this.#hints.appendOne(hint as HintType);
		});

		this.finishChange();
	};

	#propagateHints = (hints: Array<UmbHint>) => {
		if (!this.#parent) return;

		this.#parent!.initiateChange();

		const viewAlias = this.getViewAlias();

		hints.forEach((hint) => {
			let newPath = hint.path;
			// If the hint path does not already contain the parent view alias as the first entry, we add it. (This will usually happen, but some Hint Contexts does not have a view alias as they)
			if (viewAlias && newPath[0] !== viewAlias) {
				newPath = [viewAlias, ...hint.path];
			}
			this.#parent!.addOne({ ...hint, path: newPath });
		});

		// Remove hints that are not in the local hints anymore:
		const toRemove = this.#parentHints?.filter((hint) => !hints.find((m) => m.unique === hint.unique));
		if (toRemove) {
			this.#parent!.remove(toRemove.map((hint) => hint.unique));
		}

		this.#parent!.finishChange();
	};

	initiateChange() {
		this.#hints.mute();
	}
	finishChange() {
		this.#hints.unmute();
	}

	/**
	 * Add a new hint
	 * @param {HintType} hint - The hint to add
	 * @returns {HintType['unique']} Unique value of the hint
	 */
	addOne(hint: IncomingHintType): string | symbol {
		const newHint = { ...this.#scaffold.getValue(), ...hint } as unknown as HintType;
		newHint.unique ??= Symbol();
		newHint.weight ??= 0;
		newHint.text ??= '!';
		newHint.path ??= [];
		this.#hints.appendOne(newHint);
		return hint.unique!;
	}

	/**
	 * Add multiple rules
	 * @param {HintType[]} hints - Array of hints to add
	 */
	add(hints: IncomingHintType[]) {
		this.#hints.mute();
		hints.forEach((hint) => this.addOne(hint));
		this.#hints.unmute();
	}

	/**
	 * Remove a hint
	 * @param {HintType['unique']} unique Unique value of the hint to remove
	 */
	removeOne(unique: HintType['unique']) {
		this.#hints.removeOne(unique);
	}

	/**
	 * Remove multiple hints
	 * @param {HintType['unique'][]} uniques Array of unique values to remove
	 */
	remove(uniques: HintType['unique'][]) {
		this.#hints.remove(uniques);
	}

	/**
	 * Check if a hint exists
	 * @param {HintType['unique']} unique Unique value of the hint to check
	 * @returns {boolean} True if the hint exists, false otherwise
	 */
	has(unique: HintType['unique']): boolean {
		return this.#hints.getHasOne(unique);
	}

	/**
	 * Get all hints
	 * @returns {HintType[]} Array of hints
	 */
	getAll(): HintType[] {
		return this.#hints.getValue();
	}

	/**
	 * Clear all hints
	 */
	clear(): void {
		this.#hints.setValue([]);
	}

	override destroy(): void {
		super.destroy();
		if (this.#inUnprovidingState === true) {
			return;
		}
		this.unprovide();
		this.#parentHints = undefined;
		this.#parent = undefined;

		this.#hints.destroy();
	}
}
