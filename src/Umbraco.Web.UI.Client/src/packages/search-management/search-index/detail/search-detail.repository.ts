import type { UmbSearchIndex } from '../types.js';
import { UMB_SEARCH_CONTEXT } from '../global-context/search.global-context.js';
import { UmbSearchServerDataSource } from './search-detail.server.data-source.js';
import { UMB_SEARCH_DETAIL_STORE_CONTEXT } from './search-detail.store.js';

import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { SearchService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchDetailRepository extends UmbDetailRepositoryBase<UmbSearchIndex> {
	#searchContext?: typeof UMB_SEARCH_CONTEXT.TYPE;
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;
	#localize = new UmbLocalizationController(this);

	constructor(host: UmbControllerHost) {
		super(host, UmbSearchServerDataSource, UMB_SEARCH_DETAIL_STORE_CONTEXT);

		this.consumeContext(UMB_SEARCH_CONTEXT, (ctx) => (this.#searchContext = ctx));
		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (ctx) => (this.#notificationContext = ctx));
	}

	/**
	 * Rebuilds the search index with notifications.
	 * Shows a "rebuild started" notification, calls the API,
	 * and marks the user as waiting for the completion notification.
	 * Note: Confirmation modal should be shown by the caller before calling this method.
	 * @param indexAlias The alias of the index to rebuild
	 */
	async rebuildIndex(indexAlias: string): Promise<void> {
		// Show "rebuild started" notification
		this.#notificationContext?.peek('warning', {
			data: {
				title: this.#localize.term('searchManagement_rebuildConfirmHeadline'),
				message: this.#localize.term('searchManagement_rebuildStartedMessage', indexAlias),
			},
		});

		// Call API
		const { error } = await tryExecute(this, SearchService.putSearchRebuild({ query: { indexAlias } }));
		if (error) throw error;

		// Mark user waiting for completion notification
		this.#searchContext?.setUserWaitingForIndexUpdate(indexAlias, true);
	}

	override async save(model: UmbSearchIndex) {
		console.error('Saving search indexes is not supported.');
		return { data: model, error: undefined };
	}
}

export default UmbSearchDetailRepository;
