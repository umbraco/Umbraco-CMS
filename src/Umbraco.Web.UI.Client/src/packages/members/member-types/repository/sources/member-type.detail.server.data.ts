import type { MemberTypeDetails } from '../../types';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * @description - A data source for the MemberType detail that fetches data from the server
 * @export
 * @class UmbMemberTypeDetailServerDataSource
 * @implements {MemberTypeDetailDataSource}
 */
export class UmbMemberTypeDetailServerDataSource implements UmbDataSource<any, any, any, any> {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Member Type with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMediaDetailServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/member-group/${id}`)) as any;

		/*
		return tryExecuteAndNotify(
			this.#host,
			// TODO: use backend cli when available.
			fetch(`/umbraco/management/api/v1/member-group/details/${id}`)
				.then((res) => res.json())
				.then((res) => res[0] || undefined)
		);
		*/
	}

	/**
	 * @description - Creates a new MemberType scaffold
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async createScaffold() {
		const data = {} as MemberTypeDetails;
		return { data };
	}

	/**
	 * @description - Fetches a MemberType with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	requestById(id: string) {
		//return tryExecuteAndNotify(this.#host, MemberTypeResource.getMemberTypeByKey({ id }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(this.#host, fetch(`/umbraco/management/api/v1/member-group/${id}`)) as any;
	}

	/**
	 * @description - Updates a MemberType on the server
	 * @param {MemberTypeDetails} memberType
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async update(id: string, memberType: any) {
		if (!id) throw new Error('Member Type id is missing');

		const payload = { id: memberType.id, requestBody: memberType };
		//return tryExecuteAndNotify(this.#host, MemberTypeResource.putMemberTypeByKey(payload));

		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type/${memberType.id}`, {
				method: 'PUT',
				body: JSON.stringify(payload),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Inserts a new MemberType on the server
	 * @param {MemberTypeDetails} data
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async insert(data: MemberTypeDetails) {
		const requestBody = {
			name: data.name,
		};

		//return tryExecuteAndNotify(this.#host, MemberTypeResource.postMemberType({ requestBody }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type`, {
				method: 'POST',
				body: JSON.stringify(requestBody),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		) as any;
	}

	/**
	 * @description - Deletes a MemberType on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			throw new Error('Id is missing');
		}

		//return await tryExecuteAndNotify(this.#host, MemberTypeResource.deleteMemberTypeByKey({ id }));
		// TODO => use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-type/${id}`, {
				method: 'DELETE',
			})
		) as any;
	}
}
