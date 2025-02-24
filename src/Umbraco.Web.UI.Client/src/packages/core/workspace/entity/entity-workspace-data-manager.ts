import type { UmbWorkspaceDataManager } from '../data-manager/workspace-data-manager.interface.js';
import { jsonStringComparison, UmbObjectState, type MappingFunction } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * Manages the workspace data for an entity.
 * @class UmbEntityWorkspaceDataManager
 * @augments {UmbControllerBase}
 * @implements {UmbWorkspaceDataManager<ModelType>}
 * @template ModelType
 */
export class UmbEntityWorkspaceDataManager<ModelType>
	extends UmbControllerBase
	implements UmbWorkspaceDataManager<ModelType>
{
	protected readonly _persisted = new UmbObjectState<ModelType | undefined>(undefined);
	protected readonly _current = new UmbObjectState<ModelType | undefined>(undefined);

	/**
	 * Observable of the persisted data
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	public readonly persisted = this._persisted.asObservable();

	/**
	 * Observable of the current data
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	public readonly current = this._current.asObservable();

	protected _sortCurrentData(persistedData: ModelType, currentData: Partial<ModelType>) {
		// do nothing.
	}

	/**
	 * Gets persisted data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getPersisted() {
		return this._persisted.getValue();
	}

	/**
	 * Sets the persisted data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setPersisted(data: ModelType | undefined) {
		this._persisted.setValue(data);
	}

	/**
	 * Updates the persisted data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updatePersisted(partialData: Partial<ModelType>) {
		this._persisted.update(partialData);
	}

	/**
	 * Creates an observable part of the persisted data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfPersisted<ReturnType>(mappingFunction: MappingFunction<ModelType | undefined, ReturnType>) {
		return this._persisted.asObservablePart(mappingFunction);
	}

	/**
	 * Gets the current data
	 * @returns {(ModelType | undefined)}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getCurrent() {
		return this._current.getValue();
	}

	/**
	 * Sets the current data
	 * @param {(ModelType | undefined)} data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	setCurrent(data: ModelType | undefined) {
		if (data) {
			const persistedData = this._persisted.getValue();
			if (persistedData) {
				this._sortCurrentData(persistedData, data);
			}
		}
		this._current.setValue(data);
	}

	/**
	 * Updates the current data
	 * @param {Partial<ModelType>} partialData
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	updateCurrent(partialData: Partial<ModelType>) {
		if (partialData) {
			const persistedData = this._persisted.getValue();
			if (persistedData) {
				this._sortCurrentData(persistedData, partialData);
			}
		}
		this._current.update(partialData);
	}

	/**
	 * Creates an observable part of the current data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction
	 * @returns {*}
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfCurrent<ReturnType>(mappingFunction: MappingFunction<ModelType | undefined, ReturnType>) {
		return this._current.asObservablePart(mappingFunction);
	}

	/**
	 * Checks if there are unpersisted changes
	 * @returns {*}
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	getHasUnpersistedChanges() {
		const persisted = this._persisted.getValue();
		const current = this._current.getValue();
		return jsonStringComparison(persisted, current) === false;
	}

	/**
	 * Resets the current data to the persisted data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	resetCurrent() {
		this._current.setValue(this._persisted.getValue());
	}

	/**
	 * Clears the data
	 * @memberof UmbSubmittableWorkspaceDataManager
	 */
	clear() {
		this._persisted.setValue(undefined);
		this._current.setValue(undefined);
	}

	override destroy() {
		this._persisted?.destroy();
		this._current?.destroy();

		(this._persisted as any) = undefined;
		(this._current as any) = undefined;
		super.destroy();
	}
}
