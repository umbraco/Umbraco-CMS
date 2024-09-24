import type { UmbWorkspaceDataManager } from '../data-manager/workspace-data-manager.interface.js';
import { jsonStringComparison, UmbObjectState, type MappingFunction } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSubmittableWorkspaceDataManager<ModelType extends UmbEntityModel>
	extends UmbControllerBase
	implements UmbWorkspaceDataManager<ModelType>
{
	#persisted = new UmbObjectState<ModelType | undefined>(undefined);
	#current = new UmbObjectState<ModelType | undefined>(undefined);

	public readonly current = this.#current.asObservable();

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Creates an observable part
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	createObservablePart<ReturnType>(mappingFunction: MappingFunction<ModelType | undefined, ReturnType>) {
		this.#current.asObservablePart(mappingFunction);
	}

	/**
	 * Gets persisted data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getPersistedData() {
		return this.#persisted.getValue();
	}

	/**
	 * Sets the persisted data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setPersistedData(data: ModelType | undefined) {
		this.#persisted.setValue(data);
	}

	/**
	 * Updates the persisted data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updatePersistedData(partialData: Partial<ModelType>) {
		this.#persisted.update(partialData);
	}

	/**
	 * Gets the current data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getCurrentData() {
		return this.#current.getValue();
	}

	/**
	 * Sets the current data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setCurrentData(data: ModelType | undefined) {
		this.#current.setValue(data);
	}

	/**
	 * Updates the current data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updateCurrentData(partialData: Partial<ModelType>) {
		this.#current.update(partialData);
	}

	/**
	 * Checks if there are unpersisted changes
	 * @returns {*}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	hasUnpersistedChanges() {
		const persisted = this.#persisted.getValue();
		const current = this.#current.getValue();
		return jsonStringComparison(persisted, current) === false;
	}

	/**
	 * Resets the current data to the persisted data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	resetCurrentData() {
		this.#current.setValue(this.#persisted.getValue());
	}

	/**
	 * Clears the data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	clearData() {
		this.#persisted.setValue(undefined);
		this.#current.setValue(undefined);
	}

	override destroy() {
		this.#persisted.destroy();
		this.#current.destroy();
		super.destroy();
	}
}
