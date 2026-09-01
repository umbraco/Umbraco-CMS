import {
  UMB_COLLECTION_CONTEXT,
  UmbCollectionActionBase,
} from '@umbraco-cms/backoffice/collection';

export class UmbSearchCollectionReloadAction extends UmbCollectionActionBase {
  async execute(): Promise<void> {
    // Try to get the collection context
    const collectionContext = await this.getContext(UMB_COLLECTION_CONTEXT);
    if (!collectionContext) {
      throw new Error('Collection context is not available');
    }

    // Reload the collection items
    collectionContext.loadCollection();
  }
}
