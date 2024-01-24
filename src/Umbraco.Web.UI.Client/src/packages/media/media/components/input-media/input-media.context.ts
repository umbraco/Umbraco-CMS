import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMediaPickerContext extends UmbPickerInputContext<MediaItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		super(host, 'Umb.Repository.Media', UMB_MEDIA_TREE_PICKER_MODAL);
	}
}
