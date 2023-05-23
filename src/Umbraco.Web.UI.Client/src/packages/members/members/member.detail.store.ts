import { Observable } from 'rxjs';
import { umbMemberData } from '../../../mocks/data/member.data';
import type { MemberDetails } from './types';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbEntityDetailStore, UmbStoreBase } from '@umbraco-cms/backoffice/store';

/**
 * @export
 * @class UmbMemberStore
 * @extends {UmbStoreBase}
 * @description - Data Store for Members
 */
export class UmbMemberStore extends UmbStoreBase implements UmbEntityDetailStore<MemberDetails> {
	constructor(private host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_STORE_CONTEXT_TOKEN.toString(), new UmbArrayState<MemberDetails>([], (x) => x.id));
	}

	getScaffold(entityType: string, parentId: string | null) {
		return {} as MemberDetails;
	}

	/**
	 * @description - Request a Member by id. The Member is added to the store and is returned as an Observable.
	 * @param {string} id
	 * @return {*}  {(Observable<MemberDetails>)}
	 * @memberof UmbMemberStore
	 */
	getByKey(id: string): Observable<MemberDetails> {
		// tryExecuteAndNotify(this.host, MemberResource.getMemberByKey({ id })).then(({ data }) => {
		// 	if (data) {}
		// 		this.#data.appendOne(data);
		// 	}
		// });

		// temp until Resource is updated
		const member = umbMemberData.getById(id);
		if (member) {
			this._data.appendOne(member);
		}

		return createObservablePart(this._data, (members) => members.find((member) => member.id === id) as MemberDetails);
	}

	async save(member: Array<MemberDetails>): Promise<void> {
		return null as any;
	}
}

export const UMB_MEMBER_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbMemberStore>('UmbMemberStore');
