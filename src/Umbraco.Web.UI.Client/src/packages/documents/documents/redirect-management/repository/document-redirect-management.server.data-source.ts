import type {
	UmbDocumentRedirectFilterArgs,
	UmbDocumentRedirectStatusModel,
	UmbDocumentRedirectUrlModel,
} from './types.js';
import { RedirectManagementService, RedirectStatusModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { RedirectUrlResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Redirect Management feature that fetches data from the server.
 * @class UmbDocumentRedirectManagementServerDataSource
 */
export class UmbDocumentRedirectManagementServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentRedirectManagementServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to.
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the current redirect URL tracker status.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	async getStatus() {
		const { data, error } = await tryExecute(this.#host, RedirectManagementService.getRedirectManagementStatus());

		if (error || !data) {
			return { error };
		}

		const model: UmbDocumentRedirectStatusModel = {
			enabled: data.status === RedirectStatusModel.ENABLED,
			userIsAdmin: data.userIsAdmin,
		};

		return { data: model };
	}

	/**
	 * Enables or disables the redirect URL tracker.
	 * @param {boolean} enabled - Whether the tracker should be enabled.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	async setStatus(enabled: boolean) {
		const status = enabled ? RedirectStatusModel.ENABLED : RedirectStatusModel.DISABLED;
		return tryExecute(this.#host, RedirectManagementService.postRedirectManagementStatus({ query: { status } }));
	}

	/**
	 * Gets the redirects pointing to a specific document.
	 * @param {string} unique - The document unique identifier.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	async getByDocumentUnique(unique: string) {
		if (!unique) throw new Error('Unique is missing');

		const { data, error } = await tryExecute(
			this.#host,
			RedirectManagementService.getRedirectManagementById({ path: { id: unique } }),
		);

		if (error || !data) {
			return { error };
		}

		return {
			data: {
				items: (data.items ?? []).map(mapRedirectUrl),
				total: data.total,
			},
		};
	}

	/**
	 * Gets a paginated, filtered list of redirects.
	 * @param {UmbDocumentRedirectFilterArgs} args - Filter, skip and take arguments.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	async filter(args: UmbDocumentRedirectFilterArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			RedirectManagementService.getRedirectManagement({
				query: { filter: args.filter, skip: args.skip, take: args.take },
			}),
		);

		if (error || !data) {
			return { error };
		}

		return {
			data: {
				items: (data.items ?? []).map(mapRedirectUrl),
				total: data.total,
			},
		};
	}

	/**
	 * Deletes a redirect by its unique identifier.
	 * @param {string} unique - The redirect unique identifier.
	 * @returns {*}
	 * @memberof UmbDocumentRedirectManagementServerDataSource
	 */
	async delete(unique: string) {
		if (!unique) throw new Error('Unique is missing');
		return tryExecute(this.#host, RedirectManagementService.deleteRedirectManagementById({ path: { id: unique } }));
	}
}

const mapRedirectUrl = (item: RedirectUrlResponseModel): UmbDocumentRedirectUrlModel => ({
	unique: item.id,
	originalUrl: item.originalUrl,
	destinationUrl: item.destinationUrl,
	created: item.created,
	documentUnique: item.document.id,
	culture: item.culture ?? null,
});
