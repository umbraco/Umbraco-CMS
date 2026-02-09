import { UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbFolderModel } from '@umbraco-cms/backoffice/tree';
import { MemberTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for a Member Type folder that fetches data from the server
 * @class UmbMemberTypeFolderServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMemberTypeFolderServerDataSource implements UmbDetailDataSource<UmbFolderModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberTypeFolderServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMemberTypeFolderServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates a scaffold for a Member Type folder
	 * @param {Partial<UmbFolderModel>} [preset]
	 * @returns {*}
	 * @memberof UmbMemberTypeFolderServerDataSource
	 */
	async createScaffold(preset?: Partial<UmbFolderModel>) {
		const scaffold: UmbFolderModel = {
			entityType: UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
			unique: UmbId.new(),
			name: '',
			...preset,
		};

		return { data: scaffold };
	}

	/**
	 * Fetches a Member Type folder from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMemberTypeFolderServerDataSource
	 */
	async read(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			MemberTypeService.getMemberTypeFolderById({
				path: { id: unique },
			}),
		);

		if (data) {
			const mappedData = {
				entityType: UMB_MEMBER_TYPE_FOLDER_ENTITY_TYPE,
				unique: data.id,
				name: data.name,
			};

			return { data: mappedData };
		}

		return { error };
	}

	/**
	 * Creates a Member Type folder on the server
	 * @param {UmbFolderModel} model
	 * @returns {*}
	 * @memberof UmbMemberTypeFolderServerDataSource
	 */
	async create(model: UmbFolderModel, parentUnique: string | null) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Name is missing');

		const body = {
			id: model.unique,
			parent: parentUnique ? { id: parentUnique } : null,
			name: model.name,
		};

		const { error } = await tryExecute(
			this.#host,
			MemberTypeService.postMemberTypeFolder({
				body,
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Updates a Member Type folder on the server
	 * @param {UmbUpdateFolderModel} model
	 * @returns {*}
	 * @memberof UmbMemberTypeFolderServerDataSource
	 */
	async update(model: UmbFolderModel) {
		if (!model) throw new Error('Data is missing');
		if (!model.unique) throw new Error('Unique is missing');
		if (!model.name) throw new Error('Folder name is missing');

		const { error } = await tryExecute(
			this.#host,
			MemberTypeService.putMemberTypeFolderById({
				path: { id: model.unique },
				body: { name: model.name },
			}),
		);

		if (!error) {
			return this.read(model.unique);
		}

		return { error };
	}

	/**
	 * Deletes a Member Type folder on the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMemberTypeServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(
			this.#host,
			MemberTypeService.deleteMemberTypeFolderById({
				path: { id: unique },
			}),
		);
	}
}
