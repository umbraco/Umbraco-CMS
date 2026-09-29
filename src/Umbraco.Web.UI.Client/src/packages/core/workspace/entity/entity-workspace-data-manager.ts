import type { UmbWorkspaceDataManager } from '../data-manager/workspace-data-manager.interface.js';
import { jsonStringComparison, UmbObjectState, type MappingFunction } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

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

	protected _sortCurrentData<GivenType extends Partial<ModelType> = Partial<ModelType>>(
		persistedData: Partial<ModelType>,
		currentData: GivenType,
	): GivenType {
		// do nothing.
		return currentData;
	}

	/**
	 * Gets persisted data
	 * @returns {(ModelType | undefined)} The persisted data.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	getPersisted() {
		return this._persisted.getValue();
	}

	/**
	 * Sets the persisted data
	 * @param {(ModelType | undefined)} data The data to persist.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	setPersisted(data: ModelType | undefined) {
		this._persisted.setValue(data);
	}

	/**
	 * Updates the persisted data
	 * @param {Partial<ModelType>} partialData The partial data to merge into the persisted data.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	updatePersisted(partialData: Partial<ModelType>) {
		this._persisted.update(partialData);
	}

	/**
	 * Creates an observable part of the persisted data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction Function to map the persisted data to the observable part.
	 * @returns {Observable<ReturnType>} The mapped observable part.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfPersisted<ReturnType>(
		mappingFunction: MappingFunction<ModelType | undefined, ReturnType>,
	): Observable<ReturnType> {
		return this._persisted.asObservablePart(mappingFunction);
	}

	/**
	 * Gets the current data
	 * @returns {(ModelType | undefined)} The current data.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	getCurrent() {
		return this._current.getValue();
	}

	/**
	 * Sets the current data
	 * @param {(ModelType | undefined)} data The data to set as current.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	setCurrent(data: ModelType | undefined) {
		if (data) {
			const persistedData = this._persisted.getValue();
			if (persistedData) {
				data = this._sortCurrentData(persistedData, data);
			}
		}
		this._current.setValue(data);
	}

	/**
	 * Updates the current data
	 * @param {Partial<ModelType>} partialData - The partial data to merge into the current data.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	updateCurrent(partialData: Partial<ModelType>) {
		if (partialData) {
			const persistedData = this._persisted.getValue();
			if (persistedData) {
				partialData = this._sortCurrentData(persistedData, partialData);
			}
		}
		this._current.update(partialData);
	}

	/**
	 * Creates an observable part of the current data
	 * @template ReturnType
	 * @param {(MappingFunction<ModelType | undefined, ReturnType>)} mappingFunction - Maps the current data to the observed value.
	 * @returns {Observable<ReturnType>} An observable of the mapped value.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	createObservablePartOfCurrent<ReturnType>(
		mappingFunction: MappingFunction<ModelType | undefined, ReturnType>,
	): Observable<ReturnType> {
		return this._current.asObservablePart(mappingFunction);
	}

	/**
	 * Checks if there are unpersisted changes
	 * @returns {boolean} Whether the current data differs from the persisted data.
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	getHasUnpersistedChanges() {
		const persisted = this._persisted.getValue();
		const current = this._current.getValue();
		const result = jsonStringComparison(persisted, current) === false;
		// TODO: Implement developer-mode
		if (result) {
			console.warn('Changes detected based on JSON comparison between', persisted, 'and', current);
		}
		return result;
	}

	/**
	 * Resets the current data to the persisted data
	 * @memberof UmbEntityWorkspaceDataManager
	 */
	resetCurrent() {
		this._current.setValue(this._persisted.getValue());
	}

	/**
	 * Clears the data
	 * @memberof UmbEntityWorkspaceDataManager
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
