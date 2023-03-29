import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel, MediaResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Media tree that fetches data from the server
 * @export
 * @class MediaTreeServerDataSource
 * @implements {MediaTreeDataSource}
 */
export class MediaTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(keys: Array<string>) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/media/trash', {
				method: 'POST',
				body: JSON.stringify(keys),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	async moveItems(keys: Array<string>, destination: string) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/media/move', {
				method: 'POST',
				body: JSON.stringify({ keys, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of MediaTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof MediaTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof MediaTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, MediaResource.getTreeMediaRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof MediaTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetailsModel = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getTreeMediaChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof MediaTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			MediaResource.getTreeMediaItem({
				key: keys,
			})
		);
	}
}
