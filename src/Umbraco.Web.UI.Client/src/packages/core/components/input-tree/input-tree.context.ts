import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export type NodeType = 'content' | 'member' | 'media';

export class UmbNodePickerContext extends UmbPickerInputContext<ItemResponseModelBaseModel> {
	#type: NodeType = 'content';
	readonly nodeType = this.#type;

	constructor(host: UmbControllerHostElement, type?: NodeType) {
		super(host, 'Umb.Repository.Media', UMB_MEDIA_TREE_PICKER_MODAL);
	}

	setNodeType(newNodeType: NodeType) {
		this.#type = newNodeType;
	}
}
