import { UmbContextToken } from '@umbraco-cms/context-api';
import { UmbStoreBase } from '@umbraco-cms/store';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { ArrayState } from '@umbraco-cms/observable-api';
import type { MemberDetails } from '@umbraco-cms/models';

/**
 * @export
 * @class UmbMemberStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Members
 */
export class UmbMemberStore extends UmbStoreBase {
	#data = new ArrayState<MemberDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_STORE_CONTEXT_TOKEN.toString());
	}

	append(member: MemberDetails) {
		this.#data.append([member]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}
}

export const UMB_MEMBER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberStore>('UmbMemberStore');
