import type { UmbPartialSome } from '../../utils/type/index.js';
import { UmbContextBase, UmbControllerBase, type UmbClassInterface } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UUIInterfaceColor } from '@umbraco-cms/backoffice/external/uui';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import { UMB_HINT_CONTEXT } from './hint.context-token.js';
import { ReplaceStartOfPath } from '../../validation/utils/replace-start-of-path.function.js';
import type { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';

export interface UmbIncomingHintBase {
	unique?: string | symbol;
	text: string;
	weight?: number;
	color?: UUIInterfaceColor;
}

export interface UmbHint extends UmbIncomingHintBase {
	unique: string | symbol;
	path: Array<string>;
	weight: number;
}

export class UmbHintController<
	HintType extends UmbHint = UmbHint,
	IncomingHintType extends UmbIncomingHintBase = UmbPartialSome<HintType, 'unique' | 'weight' | 'path'>,
> extends UmbControllerBase {
	//
	#viewAlias?: string;
	getViewAlias(): string | undefined {
		return this.#viewAlias;
	}
	#scaffold: Partial<HintType>;
	#inUnprovidingState?: boolean;

	#parent?: UmbHintController;
	#parentHints?: Array<UmbHint>;

	readonly #hints = new UmbArrayState<HintType>([], (x) => x.unique);
	public readonly hints = this.#hints.asObservable();
	public readonly firstHint = this.#hints.asObservablePart((x) => x[0]);
	// Consider using weight to determine the visibility distance. [NL]
	//public readonly hasHints = this._hints.asObservablePart((x) => x.length > 0);

	updateScaffold(updates: Partial<HintType>) {
		this.#scaffold = { ...this.#scaffold, ...updates };
	}

	constructor(host: UmbControllerHost, args?: { viewAlias?: string; scaffold?: Partial<HintType> }) {
		super(host);

		this.#viewAlias = args?.viewAlias;
		this.#scaffold = args?.scaffold ?? {};

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

	descendingHints(viewAlias?: string): Observable<Array<UmbHint> | undefined> {
		if (viewAlias) {
			return this.#hints.asObservablePart((hints) => {
				return hints.filter((hint) => hint.path[0] === viewAlias);
			});
		} else {
			return this.hints;
		}
	}

	inherit(): void {
		this.consumeContext(UMB_HINT_CONTEXT, (parent) => {
			this.inheritFrom(parent);
		}).skipHost();
		// Notice skipHost ^^, this is because we do not want it to consume it self, as this would be a match for this consumption, instead we will look at the parent and above. [NL]
	}

	inheritFrom(parent: UmbHintController | undefined): void {
		this.#parent = parent;
		this.observe(
			parent?.descendingHints(this.#viewAlias),
			(hints) => {
				if (!hints) {
					this.#hints.clear();
					return;
				}
				this.initiateChange();
				if (this.#parentHints) {
					// Remove the local messages that does not exist in the parent anymore:
					const toRemove = this.#parentHints.filter((hint) => !hints.find((m) => m.unique === hint.unique));
					this.remove(toRemove.map((hint) => hint.unique));
				}
				this.#parentHints = hints;

				hints.forEach((hint) => {
					// Remove first entry of hint.path:
					const path = hint.path.slice(1);
					if (path === undefined) {
						throw new Error(
							'Path was not transformed correctly and can therefor not be transfered to the local validation context messages.',
						);
					}
					this.#hints.appendOne({ ...hint, path } as HintType);
				});

				this.finishChange();
			},
			'observeParentHints',
		);

		this.observe(this.hints, this.#transferHints, 'observeLocalMessages');
	}

	#transferHints = (hints: Array<UmbHint>) => {
		if (!this.#parent) return;

		this.#parent!.initiateChange();

		console.log('transferHints');

		const parentViewAlias = this.#parent.getViewAlias();

		hints.forEach((hint) => {
			console.log(hint);
			const newPath = parentViewAlias ? [parentViewAlias, ...hint.path] : hint.path;
			// Notice addOne appends the parent viewAlias to the path:
			this.#parent!.addOne({ ...hint, path: newPath });
		});

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
		const newHint = { ...this.#scaffold, ...hint } as unknown as HintType;
		newHint.unique ??= Symbol();
		newHint.weight ??= 0;
		newHint.text ??= '!';
		if (newHint.path) {
			if (this.#viewAlias && newHint.path[0] !== this.#viewAlias) {
				newHint.path = [this.#viewAlias, ...newHint.path];
			}
		} else {
			if (this.#viewAlias) {
				newHint.path = [this.#viewAlias];
			} else {
				newHint.path = [];
			}
		}
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
		super.destroy();
	}
}
