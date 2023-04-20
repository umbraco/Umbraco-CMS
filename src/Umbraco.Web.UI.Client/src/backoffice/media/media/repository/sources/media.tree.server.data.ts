import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel, MediaResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Media tree that fetches data from the server
 * @export
 * @class UmbMediaTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbMediaTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(ids: Array<string>) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify(ids),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	async moveItems(ids: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/media/move', {
				method: 'POST',
				body: JSON.stringify({ ids, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of UmbMediaTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbMediaTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaResource.getTreeMediaRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getTreeMediaChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getTreeMediaItem({
				id: ids,
			})
		);
	}
}
