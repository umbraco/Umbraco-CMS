import { UmbDataSource } from '@umbraco-cms/backoffice/repository';
import {
	ProblemDetailsModel,
	RelationTypeResource,
	RelationTypeResponseModel,
	CreateRelationTypeRequestModel,
	UpdateRelationTypeRequestModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Relation Type that fetches data from the server
 * @export
 * @class UmbRelationTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbRelationTypeServerDataSource
	implements UmbDataSource<CreateRelationTypeRequestModel, UpdateRelationTypeRequestModel, RelationTypeResponseModel>
{
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbRelationTypeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches a Relation Type with the given id from the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async get(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.getRelationTypeById({
				id,
			})
		);
	}

	/**
	 * Creates a new Relation Type scaffold
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async createScaffold(parentId: string | null) {
		const data: RelationTypeResponseModel = {};

		return { data };
	}

	/**
	 * Inserts a new Relation Type on the server
	 * @param {Document} relationType
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async insert(relationType: CreateRelationTypeRequestModel) {
		if (!relationType.id) throw new Error('RelationType id is missing');

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.postRelationType({
				requestBody: relationType,
			})
		);
	}

	/**
	 * Updates a RelationType on the server
	 * @param {RelationTypeResponseModel} relationType
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async update(id: string, relationType: UpdateRelationTypeRequestModel) {
		if (!id) throw new Error('RelationType id is missing');

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.putRelationTypeById({
				id,
				requestBody: relationType,
			})
		);
	}

	/**
	 * Deletes a Relation Type on the server
	 * @param {string} id
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async delete(id: string) {
		if (!id) {
			const error: ProblemDetailsModel = { title: 'RelationType id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.deleteRelationTypeById({
				id,
			})
		);
	}
}
