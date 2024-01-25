import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { MediaTypeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMediaTypePickerContext extends UmbPickerInputContext<MediaTypeItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_PICKER_MODAL);
	}
}
