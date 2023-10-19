import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_MEDIA_TREE_PICKER_MODAL, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export type NodeType = 'content' | 'member' | 'media';

export type StartNode = {
	type?: NodeType;
	query?: string | null;
	id?: string | null;
};

type ContextCreator = {
	repository?: string;
	token?: UmbModalToken;
};

export class UmbNodePickerContext extends UmbPickerInputContext<ItemResponseModelBaseModel> {
	#type: NodeType = 'content';
	readonly nodeType = this.#type;

	constructor(host: UmbControllerHostElement, type?: NodeType) {
		const context: ContextCreator = {};

		context.repository = 'Umb.Repository.Document';
		context.token = UMB_DOCUMENT_PICKER_MODAL;

		if (type === 'media') {
			context.repository = 'Umb.Repository.Media';
			context.token = UMB_MEDIA_TREE_PICKER_MODAL;
		}
		if (type === 'member') {
			//context.repository = 'Umb.Repository.Member';
			//context.token = UMB_MEMBER_TREE_PICKER_MODAL;
		}

		super(host, context.repository, context.token);
	}

	setNodeType(newNodeType: NodeType) {
		this.#type = newNodeType;
	}
}
