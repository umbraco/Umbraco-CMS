import type { UmbSectionItemModel } from '../../repository/index.js';
import { UMB_SECTION_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_SECTION_PICKER_MODAL } from '../../section-picker-modal/section-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionPickerContext extends UmbPickerInputContext<UmbSectionItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_ITEM_REPOSITORY_ALIAS, UMB_SECTION_PICKER_MODAL);
	}
}
