import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { PreviewService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentPreviewRepository extends UmbRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Enters preview mode.
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPreviewRepository
	 */
	async enter(): Promise<void> {
		await tryExecute(this, PreviewService.postPreview());
		return;
	}

	/**
	 * Exits preview mode.
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPreviewRepository
	 */
	async exit(): Promise<void> {
		await tryExecute(this, PreviewService.deletePreview());
		return;
	}
}
