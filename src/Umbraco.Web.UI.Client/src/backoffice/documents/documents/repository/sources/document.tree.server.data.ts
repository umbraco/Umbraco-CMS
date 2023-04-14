import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel, DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class DocumentTreeServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class DocumentTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(ids: Array<string>) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document/trash', {
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
			fetch('/umbraco/management/api/v1/document/move', {
				method: 'POST',
				body: JSON.stringify({ ids, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of DocumentTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof DocumentTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
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
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getTreeDocumentChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof DocumentTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) {
			const error: ProblemDetailsModel = { title: 'Ids are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentItem({
				id: ids,
			})
		);
	}
}
