import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	mergeObservables,
	UmbArrayState,
	UmbStringState,
	type Observable,
} from '@umbraco-cms/backoffice/observable-api';
import { ensureSlash } from '@umbraco-cms/backoffice/router';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Manages which tree entity is currently being viewed, and the trail of entities leading to it
 * @exports
 * @class UmbTreeItemActiveManager
 * @augments {UmbControllerBase}
 */
export class UmbTreeItemActiveManager extends UmbControllerBase {
	#active = new UmbArrayState<UmbEntityModel>([], (x) => x.entityType + x.unique);

	/**
	 * The trail of entities from the tree root down to the entity currently being viewed.
	 */
	readonly activeTrail = this.#active.asObservable();

	#currentLocation = new UmbStringState(window.location.pathname);

	// One listener per tree rather than one per tree item: every item asks the same question of the
	// same answer, and a tree scoped source is released together with the items observing it.
	// history.pushState runs synchronously before any router-slot dispatches this event, so the location
	// is already final on the first firing — no need to coalesce repeat firings from nested router-slots.
	#onNavigationEnd = () => this.#currentLocation.setValue(window.location.pathname);

	override hostConnected(): void {
		super.hostConnected();
		window.addEventListener('navigationend', this.#onNavigationEnd);
		this.#currentLocation.setValue(window.location.pathname);
	}

	override hostDisconnected(): void {
		super.hostDisconnected();
		window.removeEventListener('navigationend', this.#onNavigationEnd);
	}

	/**
	 * Checks if a path is the one currently open in the browser
	 * @param {Observable<string>} path The path to check, as it may change while the item is alive
	 * @returns {Observable<boolean>} True while the browser location points at the path
	 * @memberof UmbTreeItemActiveManager
	 */
	isCurrentLocation(path: Observable<string>): Observable<boolean> {
		return mergeObservables(
			[this.#currentLocation.asObservable(), path],
			// Compare with trailing slashes so /path-1 does not match /path-1-2, and allow anything
			// beyond the item path itself, such as the workspace view segment.
			([currentLocation, itemPath]) => !!itemPath && ensureSlash(currentLocation).includes(ensureSlash(itemPath)),
		);
	}

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
	 * Sets the trail of entities from the tree root down to the entity currently being viewed
	 * @param {Array<UmbEntityModel>} activeTrail The active entries, root first.
	 * @memberof UmbTreeItemActiveManager
	 * @returns {void}
	 */
	setActiveTrail(activeTrail: Array<UmbEntityModel>): void {
		this.#active.setValue(activeTrail);
	}

	/**
	 * Clears the active trail, but only when it matches the given trail
	 * @param {Array<UmbEntityModel>} activeTrail The trail that must match for the state to be cleared.
	 * @memberof UmbTreeItemActiveManager
	 * @returns {void}
	 */
	removeActiveTrailIfMatch(activeTrail: Array<UmbEntityModel>): void {
		const currentTrail = this.#active.getValue();
		// test if new trail and current trail matches:
		// Test length for a start:
		if (activeTrail.length !== currentTrail.length) return;
		// test content next:
		for (let i = 0; i < activeTrail.length; i++) {
			if (
				activeTrail[i].entityType !== currentTrail[i].entityType ||
				activeTrail[i].unique !== currentTrail[i].unique
			) {
				return;
			}
		}
		// TODO: Problem!!!! we are removing the active state, but something is loading that wants to add it...
		// then we can remove it all:
		this.#active.setValue([]);
	}

	/**
	 * Gets the trail of entities from the tree root down to the entity currently being viewed
	 * @memberof UmbTreeItemActiveManager
	 * @returns {Array<UmbEntityModel>} The active trail, root first
	 */
	getActiveTrail(): Array<UmbEntityModel> {
		return this.#active.getValue();
	}
}
