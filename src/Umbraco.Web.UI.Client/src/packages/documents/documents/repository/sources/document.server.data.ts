import type { PublishDocumentRequestModel, UnpublishDocumentRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A data source for the Document that fetches data from the server
 * @export
 * @class UmbDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDocumentServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Publish one or more variants of a Document
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async publish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('Id is missing');

		// TODO: THIS DOES NOT TAKE SEGMENTS INTO ACCOUNT!!!!!!
		const requestBody: PublishDocumentRequestModel = {
			cultures: variantIds
				.map((variant) => (variant.isCultureInvariant() ? null : variant.toCultureString()))
				.filter((x) => x !== null) as Array<string>,
		};

		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdPublish({ id, requestBody }));
	}

	/**
	 * Unpublish one or more variants of a Document
	 * @param {string} id
	 * @param {Array<UmbVariantId>} variantIds
	 * @return {*}
	 * @memberof UmbDocumentServerDataSource
	 */
	async unpublish(id: string, variantIds: Array<UmbVariantId>) {
		if (!id) throw new Error('Id is missing');

		// TODO: THIS DOES NOT TAKE SEGMENTS INTO ACCOUNT!!!!!!
		const requestBody: UnpublishDocumentRequestModel = {
			culture: variantIds.map((variant) => variant.toCultureString())[0],
		};

		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdUnpublish({ id, requestBody }));
	}

	/**
	 * Moves a Document to the recycle bin on the server
	 * @param {string} id
	 * @memberof UmbDocumentServerDataSource
	 */
	async trash(id: string) {
		if (!id) throw new Error('Document ID is missing');
		// TODO: if we get a trash endpoint, we should use that instead.
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id }));
	}

	/**
	 * Get the allowed document types for a given parent id
	 * @param {string} id
	 * @memberof UmbDocumentTypeServerDataSource
	 */
	async getAllowedDocumentTypesOf(id: string | null) {
		if (id === undefined) throw new Error('Id is missing');
		// TODO: remove when null is allowed as id.
		const hackId = id === null ? undefined : id;
		// TODO: Notice, here we need to implement pagination.
		return tryExecuteAndNotify(this.#host, DocumentResource.getDocumentAllowedDocumentTypes({ parentId: hackId }));
	}
}
