import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbStoreBase } from '@umbraco-cms/store';

/**
 * @export
 * @class UmbMemberGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Details Data Store for Member Groups
 */
export class UmbMemberGroupDetailStore
	extends UmbStoreBase
{
	#data = new ArrayState<MemberGroupDetails>([], (x) => x.key);

	constructor(host: UmbControllerHostInterface) {
		super(host, UmbMemberGroupDetailStore.name);
	}

	append(memberGroup: MemberGroupDetails) {
		this.#data.append([memberGroup]);
	}

	remove(uniques: string[]) {
		this.#data.remove(uniques);
	}	
}

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupDetailStore>(
	UmbMemberGroupDetailStore.name
);