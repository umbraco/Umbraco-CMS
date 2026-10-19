import type { UmbMemberGroupDetailModel } from '../../types.js';
import { UMB_MEMBER_GROUP_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type {
	UmbDataSourceErrorResponse,
	UmbDataSourceResponse,
	UmbDetailDataSource,
} from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { CreateMemberGroupRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MemberGroupService } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * A data source for the Member Group that fetches data from the server
 * @class UmbMemberGroupServerDataSource
 * @implements {UmbDetailDataSource}
 */
export class UmbMemberGroupServerDataSource implements UmbDetailDataSource<UmbMemberGroupDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberGroupServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberGroupServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member Group scaffold
	 * @returns { CreateMemberGroupRequestModel } The scaffolded Member Group.
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
	 * @param {string} unique - The unique identifier of the Member Group to fetch.
	 * @returns {Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>>} The Member Group, or an error.
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async read(unique: string): Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>> {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			MemberGroupService.getMemberGroupById({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		const MemberGroup: UmbMemberGroupDetailModel = {
			entityType: UMB_MEMBER_GROUP_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
		};

		return { data: MemberGroup };
	}

	/**
	 * Inserts a new Member Group on the server
	 * @param {UmbMemberGroupDetailModel} model - The Member Group to create.
	 * @returns {Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>>} The created Member Group, or an error.
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async create(model: UmbMemberGroupDetailModel): Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>> {
		if (!model) throw new Error('Member Group is missing');

		const body: CreateMemberGroupRequestModel = {
			name: model.name,
			id: model.unique,
		};

		const { data, error } = await tryExecute(
			this.#host,
			MemberGroupService.postMemberGroup({
				body,
			}),
		);

		if (data && typeof data === 'string') {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a MemberGroup on the server
	 * @param {UmbMemberGroupDetailModel} model - The Member Group to update.
	 * @returns {Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>>} The updated Member Group, or an error.
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async update(model: UmbMemberGroupDetailModel): Promise<UmbDataSourceResponse<UmbMemberGroupDetailModel>> {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		// TODO:  add type UpdateMemberGroupRequestModel
		const body: any = {
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			MemberGroupService.putMemberGroupById({
				path: { id: model.unique },
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member Group on the server
	 * @param {string} unique - The unique identifier of the Member Group to delete.
	 * @returns {Promise<UmbDataSourceErrorResponse>} Undefined if successful, or an error.
	 * @memberof UmbMemberGroupServerDataSource
	 */
	async delete(unique: string): Promise<UmbDataSourceErrorResponse> {
		if (!unique) throw new Error('Unique is missing');

		return tryExecute(
			this.#host,
			MemberGroupService.deleteMemberGroupById({
				path: { id: unique },
			}),
		);
	}
}
