import { UmbPickerInputContext } from '@umbraco-cms/backoffice/picker-input';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_DOCUMENT_PICKER_MODAL, UMB_MEDIA_TREE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';
import { ItemResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

export type NodeType = 'content' | 'member' | 'media';

export type StartNode = {
	type?: NodeType;
	query?: string | null;
	id?: string | null;
};

export class UmbNodeTreePickerContext extends UmbPickerInputContext<ItemResponseModelBaseModel> {
	#type: NodeType = 'content';

	constructor(host: UmbControllerHostElement, type: 'content' | 'media' | 'member' = 'content') {
		const context = {
			repository: 'Umb.Repository.Document',
			token: UMB_DOCUMENT_PICKER_MODAL,
		};

		// TODO => if member

		if (type === 'media') {
			context.repository = 'Umb.Repository.Media';
			context.token = UMB_MEDIA_TREE_PICKER_MODAL;
		}
		super(host, context.repository, context.token);
		this.#type = type;
	}

	getType() {
		return this.#type;
	}
}
