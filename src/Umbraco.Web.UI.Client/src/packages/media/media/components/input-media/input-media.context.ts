import type { UmbMediaItemModel } from '../../repository/item/types.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbMediaPickerContext extends UmbPickerInputContext<UmbMediaItemModel> {
	constructor(host: UmbControllerHost) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, 'Umb.Repository.Media', UMB_MEDIA_TREE_PICKER_MODAL);
	}
}
