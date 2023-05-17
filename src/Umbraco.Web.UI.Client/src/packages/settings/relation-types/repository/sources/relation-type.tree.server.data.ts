import type { UmbRelationTypeTreeDataSource } from '.';
import { RelationTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the RelationType tree that fetches data from the server
 * @export
 * @class UmbRelationTypeTreeServerDataSource
 * @implements {UmbRelationTypeTreeDataSource}
 */
export class UmbRelationTypeTreeServerDataSource implements UmbRelationTypeTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(ids: Array<string>) {
		if (!ids) {
			throw new Error('Ids are missing');
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
	 * Creates an instance of UmbRelationTypeTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, RelationTypeResource.getTreeRelationTypeRoot({}));
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbRelationTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (ids) {
			throw new Error('Ids are missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			RelationTypeResource.getRelationTypeItem({
				id: ids,
			})
		);
	}
}
