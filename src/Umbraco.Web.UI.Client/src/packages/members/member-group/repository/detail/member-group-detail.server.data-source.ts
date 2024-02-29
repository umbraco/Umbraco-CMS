import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { CreateMemberGroupRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberGroupResource } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for the Member Group that fetches data from the server
 * @export
 * @class UmbMemberGroupServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberGroupServerDataSource implements UmbDetailDataSource<UmbMemberGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberGroupServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member Group scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateMemberGroupRequestModel }
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async createScaffold() {
		const data: UmbMemberGroupDetailModel = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
		};

		return { data };
	}

	/**
	 * Fetches a Member Group with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		//TODO: Use the getById Endpoint when available
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.getItemMemberGroup({ id: [unique] }),
		);

		if (error || !data) {
			return { error };
		}

		//TODO: Use the getById Endpoint when available
		const temp = data[0];

		const MemberGroup: UmbMemberGroupDetailModel = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: temp.id,
			name: temp.name,
		};

		return { data: MemberGroup };
	}

	/**
	 * Inserts a new Member Group on the server
	 * @param {UmbMemberGroupDetailModel} model
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async create(model: UmbMemberGroupDetailModel) {
		if (!model) throw new Error('Member Group is missing');

		const requestBody: CreateMemberGroupRequestModel = {
			name: model.name,
			id: model.unique,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.postMemberGroup({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data.id);
		}

		return { error };
	}

	/**
	 * Updates a MemberGroup on the server
	 * @param {UmbMemberGroupDetailModel} MemberGroup
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async update(model: UmbMemberGroupDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		// TODO:  add type UpdateMemberGroupRequestModel
		const requestBody: any = {
			name: model.name,
		};

		/*
		const { error } = await tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.putMemberGroupById({
				id: model.unique,
				requestBody,
			}),
		);
		*/

		const { error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${model.unique}`, {
				method: 'PUT',
				body: JSON.stringify(requestBody),
			}),
		)) as any;

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member Group on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		/*
		return tryExecuteAndNotify(
			this.#host,
			MemberGroupResource.deleteMemberGroupById({
				id: unique,
			}),
		);
		*/

		const { error } = (await tryExecuteAndNotify(
			this.#host,
			fetch(`/umbraco/management/api/v1/member-group/${unique}`, {
				method: 'DELETE',
			}),
		)) as any;

		return { error };
	}
}
