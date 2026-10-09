import { UmbDocumentConfigurationRepository } from '../configuration/index.js';
import type { UmbDocumentConfigurationModel } from '../configuration/types.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

/**
 * A context for fetching and caching the document configuration.
 * @internal
 */
export class UmbDocumentConfigurationContext extends UmbContextBase implements UmbApi {
	readonly #repository = new UmbDocumentConfigurationRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the document configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns {Promise<UmbDocumentConfigurationModel | null>} A promise that resolves to the document configuration, or null if the configuration could not be fetched.
	 */
	async getDocumentConfiguration(): Promise<UmbDocumentConfigurationModel | null> {
		const { data } = await this.#repository.requestConfiguration();

		return data ?? null;
	}

	/**
	 * @deprecated Deprecated since v17. Use `getDocumentConfiguration()` instead. Will be removed in v19.
	 * @returns {Promise<UmbDocumentConfigurationModel | null>} A promise that resolves to the document configuration, or null if the configuration could not be fetched.
	 */
	fetchDocumentConfiguration(): Promise<UmbDocumentConfigurationModel | null> {
		new UmbDeprecation({
			deprecated: 'UmbDocumentConfigurationContext.fetchDocumentConfiguration()',
			removeInVersion: '19.0.0',
			solution: 'Use getDocumentConfiguration() instead.',
		}).warn();

		return this.getDocumentConfiguration();
	}
}

// Export as default to work as a global context:
export default UmbDocumentConfigurationContext;

/**
 * @internal
 */
export const UMB_DOCUMENT_CONFIGURATION_CONTEXT = new UmbContextToken<UmbDocumentConfigurationContext>(
	'UmbDocumentConfigurationContext',
);
