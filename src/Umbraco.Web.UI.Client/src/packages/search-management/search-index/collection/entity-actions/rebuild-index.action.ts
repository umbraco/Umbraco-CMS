import { UmbSearchDetailRepository } from '../../detail/search-detail.repository.js';
import { UmbSearchCollectionContext } from '../search-collection.context.js';
import { UMB_SEARCH_WORKSPACE_CONTEXT } from '../../workspace/search-workspace.context-token.js';

import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { html } from '@umbraco-cms/backoffice/external/lit';

export class UmbSearchRebuildIndexEntityAction extends UmbEntityActionBase<never> {
	readonly #repository = new UmbSearchDetailRepository(this);
	readonly #localize = new UmbLocalizationController(this);

	override async execute() {
		if (!this.args.unique) {
			throw new Error('Index alias is not provided');
		}

		// Show confirm modal first
		await umbConfirmModal(this, {
			color: 'warning',
			headline: this.#localize.term('searchManagement_rebuildConfirmHeadline'),
			content: html`${this.#localize.htmlString('#searchManagement_rebuildConfirmMessage', this.args.unique)}`,
			confirmLabel: this.#localize.term('searchManagement_rebuildConfirmLabel'),
		});

		// Set loading states BEFORE API call for immediate feedback
		// Check for workspace context (when triggered from workspace header)
		const workspaceContext = await this.getContext(UMB_SEARCH_WORKSPACE_CONTEXT).catch(() => undefined);
		if (workspaceContext) {
			workspaceContext.setState('loading');
		}

		// Set loading state for collection view (when triggered from collection)
		const context = await this.getContext(UMB_COLLECTION_CONTEXT).catch(() => undefined);
		const collectionContext = context instanceof UmbSearchCollectionContext ? context : undefined;
		if (collectionContext) {
			collectionContext.setIndexState(this.args.unique, 'loading');
		}

		try {
			// User confirmed - repository handles: notification → API call
			await this.#repository.rebuildIndex(this.args.unique);
		} catch (error) {
			// The loading state is normally cleared by the index-rebuild-completed server event, which
			// never arrives when the request itself failed - so clear it here or it spins indefinitely.
			workspaceContext?.setState('error');
			collectionContext?.setIndexState(this.args.unique, 'error');
			throw error;
		}
	}
}

export default UmbSearchRebuildIndexEntityAction;
