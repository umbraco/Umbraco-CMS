import type { UmbRelationTypeCollectionItemModel } from '../../collection/types.js';
import type { UmbRelationTypeItemModel } from '../../repository/item/types.js';
import { UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS } from '../../repository/item/constants.js';
import { UMB_RELATION_TYPE_PICKER_MODAL } from '../../modals/relation-type-picker-modal.token.js';
import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbRelationTypePickerInputContext extends UmbPickerInputContext<
	UmbRelationTypeItemModel,
	UmbRelationTypeCollectionItemModel
> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_RELATION_TYPE_ITEM_REPOSITORY_ALIAS, UMB_RELATION_TYPE_PICKER_MODAL);
	}
}
