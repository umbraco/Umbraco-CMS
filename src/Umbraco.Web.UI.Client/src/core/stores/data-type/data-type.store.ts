import { map, Observable } from 'rxjs';
import { UmbDataStoreBase } from '../store';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { ApiError, DataTypeResource, FolderTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

const isDataTypeDetails = (dataType: DataTypeDetails | FolderTreeItem): dataType is DataTypeDetails => {
	return (dataType as DataTypeDetails).data !== undefined;
};

// TODO: can we make is easy to reuse store methods across different stores?

export type UmbDataTypeStoreItemType = DataTypeDetails | FolderTreeItem;
/**
 * @export
 * @class UmbDataTypesStore
 * @extends {UmbDataStoreBase<DataTypeDetails | FolderTreeItem>}
 * @description - Data Store for Data Types
 */
export class UmbDataTypeStore extends UmbDataStoreBase<UmbDataTypeStoreItemType> {

	public readonly storeAlias = 'umbDataTypeStore';

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | null>)}
	 * @memberof UmbDataTypesStore
	 */
	getByKey(key: string): Observable<DataTypeDetails | null> {
		// TODO: use backend cli when available.
		fetch(`/umbraco/backoffice/data-type/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(map((dataTypes) => dataTypes.find((dataType) => dataType.key === key && isDataTypeDetails(dataType)) as DataTypeDetails || null));
	}

	/**
	 * @description - Save a Data Type.
	 * @param {Array<DataTypeDetails>} dataTypes
	 * @memberof UmbDataTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async save(dataTypes: Array<DataTypeDetails>): Promise<void> {
		// TODO: use backend cli when available.
		try {
			const res = await fetch('/umbraco/backoffice/data-type/save', {
				method: 'POST',
				body: JSON.stringify(dataTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.updateItems(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}

	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbDataTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async deleteItems(keys: string[]): Promise<void> {
		// TODO: use backend cli when available.
		await fetch('/umbraco/backoffice/data-type/delete', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});

		this.deleteItems(keys);
	}

	/**
	 * @description - Get the root of the tree.
	 * @return {*}  {Observable<Array<FolderTreeItem>>}
	 * @memberof UmbDataTypesStore
	 */
	getTreeRoot(): Observable<Array<FolderTreeItem>> {
		DataTypeResource.getTreeDataTypeRoot({}).then(
			(res) => {
				this.updateItems(res.items);
			},
			(e) => {
				if (e instanceof ApiError) {
					const error = e.body as ProblemDetails;
					if (e.status === 400) {
						console.log(error.detail);
					}
				}
			}
		);

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === null)));
	}
	
	/**
	 * @description - Get the children of a tree item.
	 * @param {string} key
	 * @return {*}  {Observable<Array<FolderTreeItem>>}
	 * @memberof UmbDataTypesStore
	 */
	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		DataTypeResource.getTreeDataTypeChildren({
			parentKey: key,
		}).then(
			(res) => {
				this.updateItems(res.items);
			},
			(e) => {
				if (e instanceof ApiError) {
					const error = e.body as ProblemDetails;
					if (e.status === 400) {
						console.log(error.detail);
					}
				}
			}
		);

		return this.items.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
