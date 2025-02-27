import type { UmbController } from '@umbraco-cms/backoffice/controller-api';
import type { MappingFunction, Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbWorkspaceDataManager<ModelType> extends UmbController {
	getPersisted(): ModelType | undefined;
	getCurrent(): ModelType | undefined;

	setPersisted(data: ModelType | undefined): void;
	setCurrent(data: ModelType | undefined): void;

	updatePersisted(partialData: Partial<ModelType>): void;
	updateCurrent(partialData: Partial<ModelType>): void;

	createObservablePartOfPersisted<ReturnType>(
		mappingFunction: MappingFunction<ModelType | undefined, ReturnType>,
	): Observable<ReturnType>;
	createObservablePartOfCurrent<ReturnType>(
		mappingFunction: MappingFunction<ModelType | undefined, ReturnType>,
	): Observable<ReturnType>;

	/**
	 * Checks if there are unpersisted changes
	 * @returns {*}
	 */
	getHasUnpersistedChanges(): boolean;

	/**
	 * Resets the current data to the persisted data
	 */
	resetCurrent(): void;

	/**
	 * Clears the data
	 */
	clear(): void;
}
