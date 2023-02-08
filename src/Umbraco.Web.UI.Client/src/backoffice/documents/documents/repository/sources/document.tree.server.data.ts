import type { RepositoryTreeDataSource } from '../../../../../../libs/repository/repository-tree-data-source.interface';
import { ProblemDetails, DocumentResource } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class DocumentTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class DocumentTreeServerDataSource implements RepositoryTreeDataSource {
	#host: UmbControllerHostInterface;

	// TODO: how do we handle trashed items?
	async trashItems(keys: Array<string>) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
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
			fetch('/umbraco/management/api/v1/document/move', {
				method: 'POST',
				body: JSON.stringify({ keys, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of DocumentTreeServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof DocumentTreeServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DocumentResource.getTreeDocumentRoot({}));
	}

	/**
	 * Fetches the children of a given parent key from the server
	 * @param {(string | null)} parentKey
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getChildrenOf(parentKey: string | null) {
		if (!parentKey) {
			const error: ProblemDetails = { title: 'Parent key is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getTreeDocumentChildren({
				parentKey,
			})
		);
	}

	/**
	 * Fetches the items for the given keys from the server
	 * @param {Array<string>} keys
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getItems(keys: Array<string>) {
		if (!keys) {
			const error: ProblemDetails = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getTreeDocumentItem({
				key: keys,
			})
		);
	}
}
