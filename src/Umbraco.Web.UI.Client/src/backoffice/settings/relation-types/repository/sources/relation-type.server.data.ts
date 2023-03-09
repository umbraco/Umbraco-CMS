import { RepositoryDetailDataSource } from '@umbraco-cms/repository';
import {
	ProblemDetailsModel,
	RelationTypeResource,
	RelationTypeResponseModel,
	RelationTypeCreateModel,
	RelationTypeUpdateModel,
} from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Data Type that fetches data from the server
 * @export
 * @class UmbRelationTypeServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbRelationTypeServerDataSource implements RepositoryDetailDataSource<RelationTypeResponseModel> {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbRelationTypeServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbRelationTypeServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches a Data Type with the given key from the server
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
	 * Creates a new Data Type scaffold
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async createScaffold(parentKey: string | null) {
		const data: RelationTypeResponseModel = {
			$type: '',
			parentKey: parentKey,
		};

		return { data };
	}

	/**
	 * Inserts a new Data Type on the server
	 * @param {Document} RelationType
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async insert(RelationType: RelationTypeResponseModel) {
		if (!RelationType.key) {
			const error: ProblemDetailsModel = { title: 'RelationType key is missing' };
			return { error };
		}
		const requestBody: RelationTypeCreateModel = { ...RelationType };

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<RelationTypeResponseModel>(
			this.#host,
			// TODO: avoid this any?..
			tryExecuteAndNotify(
				this.#host,
				RelationTypeResource.postRelationType({
					requestBody,
				})
			) as any
		);
	}

	/**
	 * Updates a RelationType on the server
	 * @param {RelationTypeResponseModel} RelationType
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	// TODO: Error mistake in this:
	async update(RelationType: RelationTypeResponseModel) {
		if (!RelationType.key) {
			const error: ProblemDetailsModel = { title: 'RelationType key is missing' };
			return { error };
		}

		const requestBody: RelationTypeUpdateModel = { ...RelationType };

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<RelationTypeResponseModel>(
			this.#host,
			RelationTypeResource.putRelationTypeByKey({
				key: RelationType.key,
				requestBody,
			})
		);
	}

	/**
	 * Trash a Document on the server
	 * @param {Document} Document
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async trash(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'RelationType key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<RelationTypeResponseModel>(
			this.#host,
			RelationTypeResource.deleteRelationTypeByKey({
				key,
			})
		);
	}

	/**
	 * Deletes a Data Type on the server
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbRelationTypeServerDataSource
	 */
	async delete(key: string) {
		if (!key) {
			const error: ProblemDetailsModel = { title: 'RelationType key is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		return tryExecuteAndNotify<RelationTypeResponseModel>(
			this.#host,
			RelationTypeResource.deleteRelationTypeByKey({
				key,
			})
		);
	}
}
