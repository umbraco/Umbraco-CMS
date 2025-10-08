import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { DocumentService, PreviewService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { DocumentUrlInfoModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPreviewRepository extends UmbRepositoryBase {
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
	 * Enters preview mode.
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPreviewRepository
	 * @deprecated Replaced with the Document Preview URLs feature. This will be removed in v19. [LK]
	 */
	async enter(): Promise<void> {
		new UmbDeprecation({
			removeInVersion: '19.0.0',
			deprecated: '`UmbDocumentPreviewRepository.enter()`',
			solution: 'Use `UmbDocumentPreviewRepository.getPreviewUrl()` instead',
		}).warn();

		await tryExecute(this, PreviewService.postPreview(), { disableNotifications: true });
		return;
	}

	/**
	 * Exits preview mode.
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPreviewRepository
	 */
	async exit(): Promise<void> {
		await tryExecute(this, PreviewService.deletePreview(), { disableNotifications: true });
		return;
	}
}
