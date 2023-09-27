import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { MediaItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbMediaPickerContext extends UmbPickerInputContext<MediaItemResponseModel> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.Media', UMB_MEDIA_TREE_PICKER_MODAL);
	}
}
