import { RelationTypeTreeDataSource } from '.';
import { ProblemDetailsModel, RelationTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';

import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the RelationType tree that fetches data from the server
 * @export
 * @class RelationTypeTreeServerDataSource
 * @implements {RelationTypeTreeDataSource}
 */
export class RelationTypeTreeServerDataSource implements RelationTypeTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(ids: Array<string>) {
		if (!ids) {
			const error: ProblemDetailsModel = { title: 'RelationType ids is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		/*
		return tryExecuteAndNotify<RelationType>(
			this.#host,
			RelationTypeResource.deleteRelationTypeByKey({
				id: ids,
			})
		);
		*/
		return Promise.resolve({ error: null, data: null });
	}

	async moveItems(ids: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/relation-type/move', {
				method: 'POST',
				body: JSON.stringify({ ids, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of RelationTypeTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof RelationTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof RelationTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, RelationTypeResource.getTreeRelationTypeRoot({}));
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof RelationTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (ids) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.getRelationTypeItem({
				id: ids,
			})
		);
	}
}
