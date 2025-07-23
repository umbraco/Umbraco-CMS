import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { DocumentService, type DocumentConfigurationResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

// TODO: Turn this into a Repository with a Store that holds the cache [NL]
/**
 * A context for fetching and caching the document configuration.
 * @internal
 */
export class UmbDocumentConfigurationContext extends UmbContextBase implements UmbApi {
	/**
	 * The cached document configuration.
	 */
	static #DocumentConfiguration: Promise<DocumentConfigurationResponseModel | null>;

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_CONFIGURATION_CONTEXT);
	}

	/**
	 * Get the document configuration from the server, or return the cached configuration if it has already been fetched.
	 * @returns A promise that resolves to the document configuration, or null if the configuration could not be fetched.
	 */
	getDocumentConfiguration(): Promise<DocumentConfigurationResponseModel | null> {
		return (UmbDocumentConfigurationContext.#DocumentConfiguration ??= this.fetchDocumentConfiguration());
	}

	/**
	 * Fetch the document configuration from the server.
	 * @returns A promise that resolves to the document configuration, or null if the configuration could not be fetched.
	 */
	async fetchDocumentConfiguration() {
		const { data } = await tryExecute(this, DocumentService.getDocumentConfiguration());

		return data ?? null;
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
