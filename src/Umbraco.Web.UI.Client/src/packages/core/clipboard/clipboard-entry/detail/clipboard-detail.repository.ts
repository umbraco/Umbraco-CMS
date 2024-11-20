import type { UmbClipboardEntry } from '../types.js';
import { UmbClipboardDetailLocalStorageDataSource } from './clipboard-detail.local-storage.data-source.js';
import { UMB_CLIPBOARD_DETAIL_STORE_CONTEXT } from './clipboard-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbClipboardDetailRepository extends UmbDetailRepositoryBase<UmbClipboardEntry> {
	constructor(host: UmbControllerHost) {
		super(host, UmbClipboardDetailLocalStorageDataSource, UMB_CLIPBOARD_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbClipboardEntry) {
		return super.create(model, null);
	}
}

export default UmbClipboardDetailRepository;
