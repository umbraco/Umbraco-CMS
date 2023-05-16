import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { MediaResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
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
		if (parentId === undefined) throw new Error('Parent id is missing');

		/* TODO: should we make getRootItems() internal 
		so it only is a server concern that there are two endpoints? */
		if (parentId === null) {
			return this.getRootItems();
		} else {
			return tryExecuteAndNotify(
				this.#host,
				MediaResource.getTreeMediaChildren({
					parentId,
				})
			);
		}
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbMediaTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) {
			throw new Error('Ids are missing');
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getTreeMediaItem({
				id: ids,
			})
		);
	}
}
