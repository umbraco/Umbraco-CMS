import { map, Observable } from 'rxjs';
import { UmbNodeStoreBase } from '../store';
import { MediaTypeResource, ApiError, ProblemDetails, FolderTreeItem } from '@umbraco-cms/backend-api';
import type { MediaTypeDetails } from '@umbraco-cms/models';


export type UmbMediaTypeStoreItemType = MediaTypeDetails | FolderTreeItem;
/**
 * @export
 * @class UmbMediaTypeStore
 * @extends {UmbDataStoreBase<MediaTypeDetails | EntityTreeItem>}
 * @description - Data Store for Media Types
 */
export class UmbMediaTypeStore extends UmbNodeStoreBase<UmbMediaTypeStoreItemType> {

	public readonly storeAlias = 'umbMediaTypeStore';

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<MediaTypeDetails | null>)}
	 * @memberof UmbMediaTypesStore
	 */
	getByKey(key: string): Observable<UmbMediaTypeStoreItemType | null> {
		// TODO: use backend cli when available.
		/*
		fetch(`/umbraco/backoffice/media-type/details/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.updateItems(data);
			});

		return this.items.pipe(map((mediaTypes) => mediaTypes.find((mediaType) => mediaType.key === key && isMediaTypeDetails(mediaType)) as UmbMediaTypeStoreItemType || null));
		*/
		return null as any;
	}

	/**
	 * @description - Save a Data Type.
	 * @param {Array<MediaTypeDetails | EntityTreeItem>} mediaTypes
	 * @memberof UmbMediaTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async save(mediaTypes: Array<UmbMediaTypeStoreItemType>): Promise<void> {
		// TODO: use backend cli when available.
		/*
		try {
			const res = await fetch('/umbraco/backoffice/media-type/save', {
				method: 'POST',
				body: JSON.stringify(mediaTypes),
				headers: {
					'Content-Type': 'application/json',
				},
			});
			const json = await res.json();
			this.updateItems(json);
		} catch (error) {
			console.error('Save Data Type error', error);
		}
		*/
		return null as any;
	}


	getTreeRoot(): Observable<Array<FolderTreeItem>> {
		MediaTypeResource.getTreeMediaTypeRoot({}).then(
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

	getTreeItemChildren(key: string): Observable<Array<FolderTreeItem>> {
		MediaTypeResource.getTreeMediaTypeChildren({
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
