import type { UmbSearchIndex } from '../types.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbDetailStoreBase } from '@umbraco-cms/backoffice/store';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSearchDetailStore extends UmbDetailStoreBase<UmbSearchIndex> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SEARCH_DETAIL_STORE_CONTEXT.toString());
	}
}

export default UmbSearchDetailStore;

export const UMB_SEARCH_DETAIL_STORE_CONTEXT = new UmbContextToken<UmbSearchDetailStore>('UmbSearchDetailStore');
