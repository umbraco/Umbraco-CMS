import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStoreBase } from '@umbraco-cms/backoffice/store';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { ArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { MemberTypeDetails } from '@umbraco-cms/backoffice/models';

/**
 * @export
 * @class UmbMemberTypeStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Member Types
 */
export class UmbMemberTypeStore extends UmbStoreBase {
	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN.toString(), new ArrayState<MemberTypeDetails>([], (x) => x.id));
	}

	append(MemberType: MemberTypeDetails) {
		this._data.append([MemberType]);
	}

	remove(uniques: string[]) {
		this._data.remove(uniques);
	}
}

export const UMB_MEMBER_TYPE_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberTypeStore>('UmbMemberTypeStore');
