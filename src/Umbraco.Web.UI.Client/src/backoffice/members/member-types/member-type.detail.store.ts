import type { MemberTypeDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';


export const UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeDetailStore>('UmbMemberTypeDetailStore');


/**
 * @export
 * @class UmbMemberTypeDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Member Types
 */
export class UmbMemberTypeDetailStore extends UmbStoreBase {


	#data = new ArrayState<MemberTypeDetails>([], (x) => x.key);


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_TYPE_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	/**
	 * @description - Request a Data Type by key. The Data Type is added to the store and is returned as an Observable.
	 * @param {string} key
	 * @return {*}  {(Observable<MemberTypeDetails | undefined>)}
	 * @memberof UmbMemberTypesStore
	 */
	getByKey(key: string) {
		return null as any;
	}

	// TODO: make sure UI somehow can follow the status of this action.
	/**
	 * @description - Save a Data Type.
	 * @param {Array<MemberTypeDetails>} memberTypes
	 * @memberof UmbMemberTypesStore
	 * @return {*}  {Promise<void>}
	 */
	save(data: MemberTypeDetails[]) {
		return null as any;
	}

	// TODO: How can we avoid having this in both stores?
	/**
	 * @description - Delete a Data Type.
	 * @param {string[]} keys
	 * @memberof UmbMemberTypesStore
	 * @return {*}  {Promise<void>}
	 */
	async delete(keys: string[]) {
		// TODO: use backend cli when available.
		return null as any;

		this.#data.remove(keys);
	}
}
