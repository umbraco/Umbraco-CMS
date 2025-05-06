import { UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UMB_TEMPLATE_PICKER_MODAL } from '../../modals/template-picker-modal.token.js';
import type { UmbTemplateItemModel } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTemplatePickerInputContext extends UmbPickerInputContext<UmbTemplateItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_TEMPLATE_ITEM_REPOSITORY_ALIAS, UMB_TEMPLATE_PICKER_MODAL);
	}
}
