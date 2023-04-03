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
	 * Fetches a Relation Type with the given key from the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async get(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'Key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.getRelationTypeByKey({
				key,
			})
		);
	}

	/**
	 * Creates a new Relation Type scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
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
		if (!relationType.key) throw new Error('RelationType key is missing');

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
	async update(key: string, relationType: UpdateRelationTypeRequestModel) {
		if (!key) throw new Error('RelationType key is missing');

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.putRelationTypeByKey({
				key,
				requestBody: relationType,
			})
		);
	}

	/**
	 * Deletes a Relation Type on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'RelationType key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.deleteRelationTypeByKey({
				key,
			})
		);
	}
}
