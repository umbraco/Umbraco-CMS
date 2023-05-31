import type { MemberGroupDetails } from '../../types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the MemberGroup detail that fetches data from the server
 * @export
 * @class UmbMemberGroupDetailServerDataSource
 * @implements {MemberGroupDetailDataSource}
 */
// TODO => Provide type when it is available
export class UmbMemberGroupDetailServerDataSource implements UmbDataSource<any, any, any, any> {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * @description - Creates a new MemberGroup scaffold
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async createScaffold() {
		const data: MemberGroupDetails = {
			name: '',
		} as MemberGroupDetails;

		return { data };
	}

	/**
	 * @description - Fetches a MemberGroup with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	get(id: string) {
		//return tryExecuteAndNotify(this.#host, MemberGroupResource.getMemberGroup({ id })) as any;
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/member-group/${id}`)) as any;
	}

	/**
	 * @description - Updates a MemberGroup on the server
	 * @param {MemberGroupDetails} memberGroup
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async update(id: string, memberGroup: MemberGroupDetails) {
		if (!memberGroup.id) {
			throw new Error('Member Group id is missing');
		}

		const payload = { id: memberGroup.id, requestBody: memberGroup };
		//return tryExecuteAndNotify(this.#host, MemberGroupResource.putMemberGroupByKey(payload));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${memberGroup.id}`, {
				method: 'PUT',
				body: JSON.stringify(payload),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Inserts a new MemberGroup on the server
	 * @param {MemberGroupDetails} data
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async insert(data: MemberGroupDetails) {
		const requestBody = {
			name: data.name,
		};

		//return tryExecuteAndNotify(this.#host, MemberGroupResource.postMemberGroup({ requestBody }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/`, {
				method: 'POST',
				body: JSON.stringify(requestBody),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Deletes a MemberGroup on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async trash(id: string) {
		return this.delete(id);
	}

	/**
	 * @description - Deletes a MemberGroup on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMemberGroupDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		//return await tryExecuteAndNotify(this.#host, MemberGroupResource.deleteMemberGroupByKey({ id }));
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${id}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
