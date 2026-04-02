import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DocumentService, PreviewService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbPreviewRepository extends UmbRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Gets the preview URL for a document.
	 * @param {string} unique The unique identifier of the document.
	 * @param {string} providerAlias The alias of the URL provider registered on the server.
	 * @param {string | undefined} culture The culture to preview (undefined means invariant).
	 * @param {string | undefined} segment The segment to preview (undefined means no specific segment).
	 * @returns {DocumentUrlInfoModel} The preview URLs of the document.
	 * @memberof UmbPreviewRepository
	 */
	async getPreviewUrl(
		unique: string,
		providerAlias: string,
		culture?: string,
		segment?: string,
	): Promise<DocumentUrlInfoModel> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentByIdPreviewUrl({
				path: { id: unique },
				query: { providerAlias, culture, segment },
			}),
		);

		if (error) {
			throw new Error(error.message);
		}

		return data;
	}

	/**
	 * Gets the published URL info for a document.
	 * @param {string} unique The unique identifier of the document.
	 * @param {string | undefined} culture The culture to preview (undefined means invariant).
	 * @returns {DocumentUrlInfoModel} The published URL info of the document.
	 * @memberof UmbPreviewRepository
	 */
	async getPublishedUrl(unique: string, culture?: string): Promise<DocumentUrlInfoModel | null | undefined> {
		if (!unique) return null;

		const { data, error } = await tryExecute(this, DocumentService.getDocumentUrls({ query: { id: [unique] } }));

		if (error) {
			throw new Error(error.message);
		}

		if (!data?.length) return null;

		// TODO: [LK] Review the logic here, unsure whether this is correct. When will the array have more than one item?
		const urlInfo = culture ? data[0].urlInfos.find((x) => x.culture === culture) : data[0].urlInfos[0];
		return urlInfo;
	}

	/**
	 * Exits preview mode.
	 * @returns {Promise<void>}
	 * @memberof UmbPreviewRepository
	 */
	async exit(): Promise<void> {
		await tryExecute(this, PreviewService.deletePreview(), { disableNotifications: true });
		return;
	}
}
