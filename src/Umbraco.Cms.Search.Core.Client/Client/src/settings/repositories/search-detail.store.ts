import { UMB_SEARCH_DETAIL_STORE_CONTEXT, type UmbSearchIndex } from '@umbraco-cms/search/settings';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchDetailStore extends UmbDetailStoreBase<UmbSearchIndex> {
  constructor(host: UmbControllerHost) {
    super(host, UMB_SEARCH_DETAIL_STORE_CONTEXT.toString());
  }
}
