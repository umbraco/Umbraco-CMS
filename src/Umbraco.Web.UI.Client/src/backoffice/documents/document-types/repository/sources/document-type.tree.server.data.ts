import type { UmbTreeDataSource } from '@umbraco-cms/backoffice/repository';
import { ProblemDetailsModel, DocumentTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document tree that fetches data from the server
 * @export
 * @class UmbDocumentTypeTreeServerDataSource
 * @implements {UmbTreeDataSource}
 */
export class UmbDocumentTypeTreeServerDataSource implements UmbTreeDataSource {
	#host: UmbControllerHostElement;

	// TODO: how do we handle trashed items?
	async trashItems(ids: Array<string>) {
		// TODO: use backend cli when available.
		return tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/document-type/trash', {
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
			fetch('/umbraco/management/api/v1/document-type/move', {
				method: 'POST',
				body: JSON.stringify({ ids: ids, destination }),
				headers: {
					'Content-Type': 'application/json',
				},
			})
		);
	}

	/**
	 * Creates an instance of UmbDocumentTypeTreeServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the root items for the tree from the server
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	async getRootItems() {
		return tryExecuteAndNotify(this.#host, DocumentTypeResource.getTreeDocumentTypeRoot({}));
	}

	/**
	 * Fetches the children of a given parent id from the server
	 * @param {(string | null)} parentId
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	async getChildrenOf(parentId: string | null) {
		if (!parentId) {
			const error: ProblemDetailsModel = { title: 'Parent id is missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getTreeDocumentTypeChildren({
				parentId,
			})
		);
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDocumentTypeTreeServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (ids) {
			const error: ProblemDetailsModel = { title: 'Keys are missing' };
			return { error };
		}

		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeItem({
				id: ids,
			})
		);
	}
}
