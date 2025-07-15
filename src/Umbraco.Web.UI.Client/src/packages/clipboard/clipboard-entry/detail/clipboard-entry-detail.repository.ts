import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UmbClipboardEntryDetailLocalStorageDataSource } from './clipboard-entry-detail.local-storage.data-source.js';
import { UMB_CLIPBOARD_ENTRY_DETAIL_STORE_CONTEXT } from './clipboard-entry-detail.store.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbDetailRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbClipboardEntryDetailRepository extends UmbDetailRepositoryBase<UmbClipboardEntryDetailModel> {
	constructor(host: UmbControllerHost) {
		super(host, UmbClipboardEntryDetailLocalStorageDataSource, UMB_CLIPBOARD_ENTRY_DETAIL_STORE_CONTEXT);
	}

	override async create(model: UmbClipboardEntryDetailModel) {
		return super.create(model, null);
	}
}

export default UmbClipboardEntryDetailRepository;
