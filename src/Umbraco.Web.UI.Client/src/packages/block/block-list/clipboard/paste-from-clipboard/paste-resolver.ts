import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import { UmbPropertyValueCloneController } from '@umbraco-cms/backoffice/property';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import {
	UmbClipboardEntryDetailRepository,
	UmbPasteClipboardEntryTranslateController,
	type UmbClipboardPasteResolver,
} from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardPasteResolver extends UmbControllerBase implements UmbClipboardPasteResolver {
	#detailRepository = new UmbClipboardEntryDetailRepository(this);

	async getAcceptedTypes(): Promise<string[]> {
		return ['block'];
	}
}

export { UmbBlockListClipboardPasteResolver as api };
