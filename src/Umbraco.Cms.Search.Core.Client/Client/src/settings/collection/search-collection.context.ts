import type { UmbSearchIndex, UmbSearchIndexState } from '../types.js';
import { UMB_SEARCH_COLLECTION_VIEW_ALIAS, UMB_SEARCH_CONTEXT } from '@umbraco-cms/search/global';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchCollectionContext extends UmbDefaultCollectionContext<UmbSearchIndex, never> {
  constructor(host: UmbControllerHostElement) {
    super(host, UMB_SEARCH_COLLECTION_VIEW_ALIAS);

    this.consumeContext(UMB_SEARCH_CONTEXT, (searchContext) => {
      if (!searchContext) return;
      this.observe(
        searchContext.indexRebuilt,
        (indexAlias) => {
          if (!indexAlias) return;
          this.loadCollection();
        },
        'index-rebuild-completed-observer',
      );
    });
  }

  /**
   * Sets the index state for immediate UI feedback.
   * This is used when triggering actions that take time (e.g., rebuild).
   * The state will be replaced by server-driven state when the collection reloads.
   */
  setIndexState(indexAlias: string, state: UmbSearchIndexState) {
    this._items.updateOne(indexAlias, { state });
  }
}
