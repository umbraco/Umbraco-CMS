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
	async trashItems(keys: Array<string>) {
		if (!keys) {
			const error: ProblemDetailsModel = { title: 'RelationType keys is missing' };
			return { error };
		}

		// TODO: use resources when end point is ready:
		/*
		return tryExecuteAndNotify<RelationType>(
			this.#host,
			RelationTypeResource.deleteRelationTypeByKey({
				key: keys,
			})
		);
		*/
		return Promise.resolve({ error: null, data: null });
	}

	async moveItems(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/relation-type/move', {
				method: 'POST',
				body: JSON.stringify({ keys, destination }),
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
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof RelationTypeTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.getTreeRelationTypeItem({
				key: keys,
			})
		);
	}
}
