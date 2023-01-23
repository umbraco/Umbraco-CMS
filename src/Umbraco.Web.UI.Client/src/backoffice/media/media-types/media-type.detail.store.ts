import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { UniqueArrayBehaviorSubject } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/stores/store-base';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeDetailStore>('UmbMediaTypeDetailStore');


/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeDetailStore extends UmbStoreBase {


	private _data = new UniqueArrayBehaviorSubject<DataTypeDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeDetails | undefined>)}
	 * @memberof UmbMediaTypesStore
	 */
	getByKey(key: string) {
		return null as any;
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a Data Type.
	 * @param {Array<DataTypeDetails>} dataTypes
	 * @memberof UmbMediaTypesStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: DataTypeDetails[]) {
		return null as any;
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Media Type.
	 * @param {string[]} keys
	 * @memberof UmbMediaTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		this._data.remove(keys);
	}
}
