import type { UmbSectionItemModel } from '../../repository/types.js';
import { UMB_SECTION_PICKER_MODAL } from '../../section-picker-modal/section-picker-modal.token.js';
import { UMB_SECTION_ITEM_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbSectionPickerInputContext extends UmbPickerInputContext<UmbSectionItemModel> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_SECTION_ITEM_REPOSITORY_ALIAS, UMB_SECTION_PICKER_MODAL);
	}
}
