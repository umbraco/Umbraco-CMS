import type { UmbEntityExpansionEntryModel, UmbEntityExpansionModel } from './types.js';
import {
	UmbExpansionEntityExpandedEvent,
	UmbExpansionEntityCollapsedEvent,
	UmbExpansionChangeEvent,
} from './events/index.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages an expansion state
 * @exports
 * @class UmbEntityExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbEntityExpansionManager extends UmbControllerBase {
	#expansion = new UmbArrayState<UmbEntityExpansionEntryModel>([], (x) => x.entityType + x.unique);
	expansion = this.#expansion.asObservable();

	/**
	 * Checks if an entity is expanded
	 * @param {UmbEntityModel} entity The entity to check
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @returns {Observable<boolean>} True if the entity is expanded
	 * @memberof UmbEntityExpansionManager
	 */
	isExpanded(entity: UmbEntityModel): Observable<boolean> {
		return this.#expansion.asObservablePart((entries) =>
			entries?.some((entry) => entry.entityType === entity.entityType && entry.unique === entity.unique),
		);
	}

	/**
	 * Sets the expansion state
	 * @param {UmbEntityExpansionModel | undefined} expansion The expansion state
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	setExpansion(expansion: UmbEntityExpansionModel): void {
		this.#expansion.setValue(expansion);
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Gets the expansion state
	 * @memberof UmbEntityExpansionManager
	 * @returns {UmbEntityExpansionModel} The expansion state
	 */
	getExpansion(): UmbEntityExpansionModel {
		return this.#expansion.getValue();
	}

	/**
	 * Expands an entity
	 * @param {UmbEntityExpansionEntryModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @param {UmbEntityModel} entity.target The target entity to look up
	 * @param {string} entity.target.entityType The target entity type
	 * @param {string} entity.target.unique The target unique key
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async expandItem(entity: UmbEntityExpansionEntryModel): Promise<void> {
		this.#expansion.appendOne(entity);
		this.getHostElement()?.dispatchEvent(new UmbExpansionEntityExpandedEvent(entity));
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Expands multiple entities
	 * @param {UmbEntityExpansionModel} entities The entities to open
	 * @memberof UmbEntityExpansionManager
	 * @returns {void}
	 */
	public expandItems(entities: UmbEntityExpansionModel): void {
		if (!entities || entities.length === 0) return;
		this.#expansion.append(entities);
		entities.forEach((entity) => {
			this.getHostElement()?.dispatchEvent(new UmbExpansionEntityExpandedEvent(entity));
		});
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses an entity
	 * @param {UmbEntityExpansionEntryModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @param {UmbEntityModel} entity.target The target entity to look up
	 * @param {string} entity.target.entityType The target entity type
	 * @param {string} entity.target.unique The target unique key
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseItem(entity: UmbEntityModel): Promise<void> {
		this.#expansion.filter((x) => x.entityType !== entity.entityType || x.unique !== entity.unique);
		this.getHostElement()?.dispatchEvent(new UmbExpansionEntityCollapsedEvent(entity));
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}

	/**
	 * Collapses all expanded entities
	 * @memberof UmbEntityExpansionManager
	 * @returns {Promise<void>}
	 */
	public async collapseAll(): Promise<void> {
		this.#expansion.setValue([]);
		this.getHostElement()?.dispatchEvent(new UmbExpansionChangeEvent());
	}
}
