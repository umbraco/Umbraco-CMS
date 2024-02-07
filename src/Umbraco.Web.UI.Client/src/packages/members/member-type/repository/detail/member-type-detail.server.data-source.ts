import type { UmbMemberTypeDetailModel, UmbMemberTypePropertyModel } from '../../types.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateMemberTypeRequestModel,
	MemberTypeModelBaseModel,
	UpdateMemberTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { MemberTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Member Type that fetches data from the server
 * @export
 * @class UmbMemberTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberTypeServerDataSource implements UmbDetailDataSource<UmbMemberTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberTypeServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a new Member Type scaffold
	 * @param {(string | null)} parentUnique
	 * @return { CreateMemberTypeRequestModel }
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async createScaffold(parentUnique: string | null) {
		const data: UmbMemberTypeDetailModel = {
			entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			parentUnique,
			name: '',
			alias: '',
			description: '',
			icon: '',
			allowedAsRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			properties: [],
			containers: [],
			allowedContentTypes: [],
			compositions: [],
		};

		return { data };
	}

	/**
	 * Fetches a Member Type with the given id from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecuteAndNotify(this.#host, MemberTypeResource.getMemberTypeById({ id: unique }));

		if (error || !data) {
			return { error };
		}

		// TODO: make data mapper to prevent errors
		const memberType: UmbMemberTypeDetailModel = {
			entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
			unique: data.id,
			parentUnique: null,
			name: data.name,
			alias: data.alias,
			description: data.description || null,
			icon: data.icon,
			allowedAsRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			properties: data.properties.map((property) => {
				return {};
			}),
			containers: data.containers,
			allowedContentTypes: [],
			compositions: data.compositions,
		};

		return { data: memberType };
	}

	/**
	 * Inserts a new Member Type on the server
	 * @param {UmbMemberTypeDetailModel} model
	 * @return {*}
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async create(model: UmbMemberTypeDetailModel) {
		if (!model) throw new Error('Member Type is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: CreateMemberTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAsRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties,
			containers: model.containers,
			id: model.unique,
			Compositions: model.compositions,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.postMemberType({
				requestBody,
			}),
		);

		if (data) {
			return this.read(data);
		}

		return { error };
	}

	/**
	 * Updates a MemberType on the server
	 * @param {UmbMemberTypeDetailModel} MemberType
	 * @return {*}
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async update(model: UmbMemberTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const requestBody: UpdateMemberTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAsRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {}),
			containers: model.containers,
			id: model.unique,
			Compositions: model.compositions,
		};

		const { error } = await tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.putMemberTypeById({
				id: model.unique,
				requestBody,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member Type on the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			MemberTypeResource.deleteMemberTypeById({
				id: unique,
			}),
		);
	}
}
