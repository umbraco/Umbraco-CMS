import type { UmbDocumentRedirectFilterArgs } from './types.js';
import { UmbDocumentRedirectManagementServerDataSource } from './document-redirect-management.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * Repository for managing document redirect URLs.
 * @class UmbDocumentRedirectManagementRepository
 * @augments {UmbControllerBase}
 */
export class UmbDocumentRedirectManagementRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbDocumentRedirectManagementServerDataSource(this);

	/**
	 * Gets the current redirect URL tracker status.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementRepository
	 */
	async requestStatus() {
		return this.#dataSource.getStatus();
	}

	/**
	 * Enables or disables the redirect URL tracker.
	 * @param {boolean} enabled - Whether the tracker should be enabled.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementRepository
	 * @deprecated Deprecated since v17. The backend endpoint is now a no-op; set the
	 *   `Umbraco:CMS:WebRouting:DisableRedirectUrlTracking` configuration key instead.
	 *   Scheduled for removal in Umbraco 19.
	 */
	async setStatus(enabled: boolean) {
		new UmbDeprecation({
			deprecated: 'UmbDocumentRedirectManagementRepository.setStatus()',
			removeInVersion: '19.0.0',
			solution:
				'The backend endpoint is now a no-op. Set the Umbraco:CMS:WebRouting:DisableRedirectUrlTracking configuration key instead.',
		}).warn();

		return this.#dataSource.setStatus(enabled);
	}

	/**
	 * Gets the redirects pointing to a specific document.
	 * @param {string} unique - The document unique identifier.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementRepository
	 */
	async requestByDocumentUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#dataSource.getByDocumentUnique(unique);
	}

	/**
	 * Gets a paginated, filtered list of redirects.
	 * @param {UmbDocumentRedirectFilterArgs} [args] - Optional filter, skip and take arguments.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementRepository
	 */
	async requestRedirects(args: UmbDocumentRedirectFilterArgs = {}) {
		return this.#dataSource.filter(args);
	}

	/**
	 * Deletes a redirect by its unique identifier.
	 * @param {string} unique - The redirect unique identifier.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementRepository
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return this.#dataSource.delete(unique);
	}
}

export { UmbDocumentRedirectManagementRepository as api };
