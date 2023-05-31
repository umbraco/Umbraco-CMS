import type { MemberTypeDetails } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * @export
 * @class UmbMemberTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Types
 */
export class UmbMemberTypeStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<MemberTypeDetails>([], (x) => x.id));
	}

	append(MemberType: MemberTypeDetails) {
		this._data.append([MemberType]);
	}

	/**
	 * Retrieve a member type from the store
	 * @param {string} id
	 * @memberof UmbMemberTypeStore
	 */
	byId(id: MemberTypeDetails['id']) {
		return this._data.getObservablePart((x) => x.find((y) => y.id === id));
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeStore>('UmbMemberTypeStore');
