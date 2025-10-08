import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Manages the expansion state of a tree
 * @exports
 * @class UmbTreeItemActiveManager
 * @augments {UmbControllerBase}
 */
export class UmbTreeItemActiveManager extends UmbControllerBase {
	#active = new UmbArrayState<UmbEntityModel>([], (x) => x.entityType + x.unique);
	readonly active = this.#active.asObservable();

	/**
	 * Checks if an entity is active
	 * @param {UmbEntityModel} entity The entity to check
	 * @returns {Observable<boolean>} True if the entity is active
	 * @memberof UmbTreeItemActiveManager
	 */
	isActive(entity: UmbEntityModel): Observable<boolean> {
		return this.#active.asObservablePart((entities) => {
			const index = entities.findIndex((e) => e.entityType === entity.entityType && e.unique === entity.unique);
			return index === entities.length - 1;
		});
	}

	/**
	 * Checks if an descendant entity is active
	 * @param {UmbEntityModel} entity The entity to check
	 * @returns {Observable<boolean>} True if a descendant entity is active
	 * @memberof UmbTreeItemActiveManager
	 */
	hasActiveDescendants(entity: UmbEntityModel): Observable<boolean> {
		return this.#active.asObservablePart((entities) => {
			const index = entities.findIndex((e) => e.entityType === entity.entityType && e.unique === entity.unique);
			return index > -1 && index < entities.length - 1;
		});
	}
	/**
	 * Checks if an descendant entity is active
	 * @param {UmbEntityModel} entity The entity to check
	 * @returns {boolean} True if a descendant entity is active
	 * @memberof UmbTreeItemActiveManager
	 */
	getHasActiveDescendants(entity: UmbEntityModel): boolean {
		return this.#active.getValue().some((e) => e.entityType === entity.entityType && e.unique === entity.unique);
	}

	/**
	 * Sets the active chain state
	 * @param {Array<UmbEntityModel>} activeChain The active entries.
	 * @memberof UmbTreeItemActiveManager
	 * @returns {void}
	 */
	setActive(activeChain: Array<UmbEntityModel>): void {
		this.#active.setValue(activeChain);
	}

	/**
	 * Sets the active chain state
	 * @param {Array<UmbEntityModel>} activeChain The active entries.
	 * @memberof UmbTreeItemActiveManager
	 * @returns {void}
	 */
	removeActiveIfMatch(activeChain: Array<UmbEntityModel>): void {
		const currentChain = this.#active.getValue();
		// test if new chain and current chain matches:
		// Test length for a start:
		if (activeChain.length !== currentChain.length) return;
		// test content next:
		for (let i = 0; i < activeChain.length; i++) {
			if (
				activeChain[i].entityType !== currentChain[i].entityType ||
				activeChain[i].unique !== currentChain[i].unique
			) {
				return;
			}
		}
		// TODO: Problem!!!! we are removing the active state, but something is loading that wants to add it...
		// then we can remove it all:
		this.#active.setValue([]);
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbTreeItemActiveManager
	 * @returns {Array<UmbEntityModel>} The expansion state
	 */
	getActive(): Array<UmbEntityModel> {
		return this.#active.getValue();
	}
}
