import { map, Observable } from 'rxjs';
import { UmbEntityStore } from '../entity.store';
import { UmbDataStoreBase } from '../store';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { ApiError, DataTypeResource, FolderTreeItem, ProblemDetails } from '@umbraco-cms/backend-api';

/**
 * @export
 * @class UmbDataTypeStore
 * @extends {UmbDataStoreBase<DataTypeDetails | FolderTreeItem>}
 * @description - Data Store for Data Types
 */
export class UmbDataTypeStore extends UmbDataStoreBase<DataTypeDetails | FolderTreeItem> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | null>)}
	 * @memberof UmbDataTypeStore
	 */
	getByKey(key: string): Observable<DataTypeDetails | FolderTreeItem | null> {
		// TODO: use Fetcher API.
		// TODO: only fetch if the data type is not in the store?
		fetch(`/umbraco/backoffice/data-type/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(map((dataTypes) => dataTypes.find((dataType) => dataType.key === key) || null));
	}

	/**
	 * @description - Save a Data Type.
	 * @param {Array<DataTypeDetails>} dataTypes
	 * @memberof UmbDataTypeStore
	 * @return {*}  {Promise<void>}
	 */
	async save(dataTypes: Array<DataTypeDetails>): Promise<void> {
		// TODO: use Fetcher API.
		try {
			const res = await fetch('/umbraco/backoffice/data-type/save', {
				method: 'POST',
				body: JSON.stringify(dataTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.update(json);
			this._entityStore.update(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
	}

	/**
	 * @description - Add a Data Type to the recycle bin.
	 * @param {string[]} keys
	 * @memberof UmbDataTypeStore
	 * @return {*}  {Promise<void>}
	 */
	async trash(keys: string[]): Promise<void> {
		const res = await fetch('/umbraco/backoffice/data-type/trash', {
			method: 'POST',
			body: JSON.stringify(keys),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const data = await res.json();
		this.update(data);
		this._entityStore.update(data);
	}

	getTreeRoot(): Observable<Array<FolderTreeItem>> {
		DataTypeResource.getTreeDataTypeRoot({}).then(
			(res) => {
				this.update(res.items);
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

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		DataTypeResource.getTreeDataTypeChildren({
			parentKey: key,
		}).then(
			(res) => {
				this.update(res.items);
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
