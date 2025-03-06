import type { UmbTreeExpansionModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { appendToFrozenArray, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages the expansion state of a tree
 * @exports
 * @class UmbTreeExpansionManager
 * @augments {UmbControllerBase}
 */
export class UmbTreeExpansionManager extends UmbControllerBase {
	#expansion = new UmbObjectState<UmbTreeExpansionModel | undefined>(undefined);
	expansion = this.#expansion.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
	}

	setExpansion(expansion: UmbTreeExpansionModel | undefined): void {
		this.#expansion.setValue(expansion);
	}

	getExpansion(): UmbTreeExpansionModel | undefined {
		return this.#expansion.getValue();
	}

	/**
	 * Opens a child tree item
	 * @param {UmbEntityModel} entity The entity to open
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeExpansionManager
	 * @returns {void}
	 */
	public expandItem(entity: UmbEntityModel): void {
		const currentValue = this.#expansion.getValue() ?? [];
		const newValue = appendToFrozenArray(currentValue, entity, (x) => x?.unique);
		this.#expansion.setValue(newValue);
	}

	/**
	 * Closes a child tree item
	 * @param {UmbEntityModel} entity The entity to close
	 * @param {string} entity.entityType The entity type
	 * @param {string} entity.unique The unique key
	 * @memberof UmbTreeExpansionManager
	 * @returns {void}
	 */
	public collapseItem(entity: UmbEntityModel): void {
		const currentValue = this.#expansion.getValue() ?? [];
		const newValue = currentValue.filter((x) => x.entityType !== entity.entityType || x.unique !== entity.unique);
		this.#expansion.setValue(newValue);
	}

	/**
	 * Closes all child tree items
	 * @memberof UmbTreeExpansionManager
	 * @returns {void}
	 */
	public collapseAll(): void {
		this.#expansion.setValue([]);
	}
}
