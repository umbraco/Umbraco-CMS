import type { UmbWorkspaceDataManager } from '../data-manager/workspace-data-manager.interface.js';
import { jsonStringComparison, UmbObjectState, type MappingFunction } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Manages the workspace data for an entity.
 * @class UmbEntityWorkspaceDataManager
 * @augments {UmbControllerBase}
 * @implements {UmbWorkspaceDataManager<ModelType>}
 * @template ModelType
 */
export class UmbEntityWorkspaceDataManager<ModelType extends UmbEntityModel>
	extends UmbControllerBase
	implements UmbWorkspaceDataManager<ModelType>
{
	#persisted = new UmbObjectState<ModelType | undefined>(undefined);
	#current = new UmbObjectState<ModelType | undefined>(undefined);

	/**
	 * Observable of the persisted data
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	public readonly persisted = this.#persisted.asObservable();

	/**
	 * Observable of the current data
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	public readonly current = this.#current.asObservable();

	/**
	 * Gets persisted data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getPersisted() {
		return this.#persisted.getValue();
	}

	/**
	 * Sets the persisted data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setPersisted(data: ModelType | undefined) {
		this.#persisted.setValue(data);
	}

	/**
	 * Updates the persisted data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updatePersisted(partialData: Partial<ModelType>) {
		this.#persisted.update(partialData);
	}

	/**
	 * Creates an observable part of the persisted data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfPersisted<ReturnType>(mappingFunction: MappingFunction<ModelType | undefined, ReturnType>) {
		return this.#persisted.asObservablePart(mappingFunction);
	}

	/**
	 * Gets the current data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getCurrent() {
		return this.#current.getValue();
	}

	/**
	 * Sets the current data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setCurrent(data: ModelType | undefined) {
		this.#current.setValue(data);
	}

	/**
	 * Updates the current data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updateCurrent(partialData: Partial<ModelType>) {
		this.#current.update(partialData);
	}

	/**
	 * Creates an observable part of the current data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfCurrent<ReturnType>(mappingFunction: MappingFunction<ModelType | undefined, ReturnType>) {
		return this.#current.asObservablePart(mappingFunction);
	}

	/**
	 * Checks if there are unpersisted changes
	 * @returns {*}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getHasUnpersistedChanges() {
		const persisted = this.#persisted.getValue();
		const current = this.#current.getValue();
		return jsonStringComparison(persisted, current) === false;
	}

	/**
	 * Resets the current data to the persisted data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	resetCurrent() {
		this.#current.setValue(this.#persisted.getValue());
	}

	/**
	 * Clears the data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	clear() {
		this.#persisted.setValue(undefined);
		this.#current.setValue(undefined);
	}

	override destroy() {
		this.#persisted.destroy();
		this.#current.destroy();
		super.destroy();
	}
}
