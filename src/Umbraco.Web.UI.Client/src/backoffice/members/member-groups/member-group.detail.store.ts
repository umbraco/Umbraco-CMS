import { Observable } from 'rxjs';
import type { MemberGroupDetails } from '@umbraco-cms/models';
import { UmbContextToken } from '@umbraco-cms/context-api';
import { ArrayState } from '@umbraco-cms/observable-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbStoreBase } from '@umbraco-cms/store';

export const UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberGroupDetailStore>('UmbMemberGroupDetailStore');

/**
 * @export
 * @class UmbMemberGroupDetailStore
 * @extends {UmbStoreBase}
 * @description - Detail Data Store for Member Groups
 */
export class UmbMemberGroupDetailStore extends UmbStoreBase {


	#groups = new ArrayState<MemberGroupDetails>([], x => x.key);
	public groups = this.#groups.asObservable();


	constructor(host: UmbControllerHostInterface) {
		super(host, UMB_MEMBER_GROUP_DETAIL_STORE_CONTEXT_TOKEN.toString());
	}

	getByKey(key: string): Observable<MemberGroupDetails | null> {
		return null as any;
	}

	async save(mediaTypes: Array<MemberGroupDetails>): Promise<void> {
		return null as any;
	}
}
