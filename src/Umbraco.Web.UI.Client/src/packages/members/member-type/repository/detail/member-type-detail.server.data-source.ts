import type { UmbMemberTypeDetailModel } from '../../types.js';
import { UMB_MEMBER_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbManagementApiMemberTypeDetailDataRequestManager } from './server-data-source/member-type-detail.server.request-manager.js';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';
import type {
	CreateMemberTypeRequestModel,
	MemberTypeResponseModel,
	UpdateMemberTypeRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbPropertyTypeContainerModel } from '@umbraco-cms/backoffice/content-type';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source for the Member Type that fetches data from the server
 * @class UmbMemberTypeDetailServerDataSource
 * @implements {UmbDetailDataSource<UmbMemberTypeDetailModel>}
 */
export class UmbMemberTypeDetailServerDataSource
	extends UmbControllerBase
	implements UmbDetailDataSource<UmbMemberTypeDetailModel>
{
	#detailRequestManager = new UmbManagementApiMemberTypeDetailDataRequestManager(this);

	/**
	 * Creates a new Member Type scaffold
	 * @param {Partial<UmbMemberTypeDetailModel>} [preset] - Optional preset data to merge with the scaffold
	 * @returns { CreateMemberTypeRequestModel }
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async createScaffold(preset: Partial<UmbMemberTypeDetailModel> = {}) {
		const data: UmbMemberTypeDetailModel = {
			entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			alias: '',
			description: '',
			icon: 'icon-user',
			allowedAtRoot: false,
			variesByCulture: false,
			variesBySegment: false,
			isElement: false,
			properties: [],
			containers: [],
			allowedContentTypes: [],
			compositions: [],
			collection: null,
			...preset,
		};

		return { data };
	}

	/**
	 * Fetches a Member Type with the given id from the server
	 * @param {string} unique - The unique identifier of the Member Type to fetch
	 * @returns {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await this.#detailRequestManager.read(unique);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Fetches multiple Member Types by their unique IDs from the server
	 * @param {Array<string>} uniques - The unique IDs of the member types to fetch
	 * @returns {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async readMany(uniques: Array<string>) {
		if (!uniques || uniques.length === 0) {
			return { data: [] };
		}

		const { data, error } = await this.#detailRequestManager.readMany(uniques);

		return {
			data: data?.items?.map((item) => this.#mapServerResponseModelToEntityDetailModel(item)),
			error,
		};
	}

	/**
	 * Inserts a new Member Type on the server
	 * @param {UmbMemberTypeDetailModel} model - Member Type Model
	 * @returns {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async create(model: UmbMemberTypeDetailModel, parentUnique: string | null = null) {
		if (!model) throw new Error('Member Type is missing');

		// TODO: make data mapper to prevent errors
		const body: CreateMemberTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container ? { id: property.container.id } : null,
					sortOrder: property.sortOrder,
					alias: property.alias,
					isSensitive: property.isSensitive ?? false,
					visibility: property.visibility ?? { memberCanEdit: false, memberCanView: false },
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers,
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			compositions: model.compositions.map((composition) => {
				return {
					memberType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
		};

		const { data, error } = await this.#detailRequestManager.create(body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Updates a MemberType on the server
	 * @param { UmbMemberTypeDetailModel } model - Member Type Model
	 * @returns {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async update(model: UmbMemberTypeDetailModel) {
		if (!model.unique) throw new Error('Unique is missing');

		// TODO: make data mapper to prevent errors
		const body: UpdateMemberTypeRequestModel = {
			alias: model.alias,
			name: model.name,
			description: model.description,
			icon: model.icon,
			allowedAsRoot: model.allowedAtRoot,
			variesByCulture: model.variesByCulture,
			variesBySegment: model.variesBySegment,
			isElement: model.isElement,
			properties: model.properties.map((property) => {
				return {
					id: property.unique,
					container: property.container ? { id: property.container.id } : null,
					sortOrder: property.sortOrder,
					isSensitive: property.isSensitive ?? false,
					visibility: property.visibility ?? { memberCanEdit: false, memberCanView: false },
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { id: property.dataType.unique },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
				};
			}),
			containers: model.containers,
			compositions: model.compositions.map((composition) => {
				return {
					memberType: { id: composition.contentType.unique },
					compositionType: composition.compositionType,
				};
			}),
		};

		const { data, error } = await this.#detailRequestManager.update(model.unique, body);

		return { data: data ? this.#mapServerResponseModelToEntityDetailModel(data) : undefined, error };
	}

	/**
	 * Deletes a Member Type on the server
	 * @param {string} unique - The unique identifier of the Member Type to delete
	 * @returns {*}
	 * @memberof UmbMemberTypeDetailServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#detailRequestManager.delete(unique);
	}

	#mapServerResponseModelToEntityDetailModel(data: MemberTypeResponseModel): UmbMemberTypeDetailModel {
		return {
			entityType: UMB_MEMBER_TYPE_ENTITY_TYPE,
			unique: data.id,
			name: data.name,
			alias: data.alias,
			description: data.description ?? '',
			icon: data.icon,
			allowedAtRoot: data.allowedAsRoot,
			variesByCulture: data.variesByCulture,
			variesBySegment: data.variesBySegment,
			isElement: data.isElement,
			properties: data.properties.map((property) => {
				return {
					id: property.id,
					unique: property.id,
					container: property.container ? { id: property.container.id } : null,
					sortOrder: property.sortOrder,
					alias: property.alias,
					name: property.name,
					description: property.description,
					dataType: { unique: property.dataType.id },
					variesByCulture: property.variesByCulture,
					variesBySegment: property.variesBySegment,
					validation: property.validation,
					appearance: property.appearance,
					visibility: property.visibility,
					isSensitive: property.isSensitive,
				};
			}),
			containers: data.containers as UmbPropertyTypeContainerModel[],
			allowedContentTypes: [],
			compositions: data.compositions.map((composition) => {
				return {
					contentType: { unique: composition.memberType.id },
					compositionType: composition.compositionType,
				};
			}),
			collection: data.collection ? { unique: data.collection.id } : null,
		};
	}
}
