import type { MediaTypeDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';

export const UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMediaTypeDetailStore>(
	'UmbMediaTypeDetailStore'
);

/**
 * @export
 * @class UmbMediaTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Media Types
 */
export class UmbMediaTypeDetailStore extends UmbStoreBase implements UmbEntityDetailStore<MediaTypeDetails> {
	private _data = new ArrayState<MediaTypeDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEDIA_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getScaffold(entityType: string, parentKey: string | null) {
		return {} as MediaTypeDetails;
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<DataTypeModel | undefined>)}
	 * @memberof UmbMediaTypesStore
	 */
	getByKey(key: string) {
		return null as any;
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a Media Type.
	 * @param {Array<MediaTypeDetails>} mediaTypes
	 * @memberof UmbMediaTypesStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: MediaTypeDetails[]) {
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
