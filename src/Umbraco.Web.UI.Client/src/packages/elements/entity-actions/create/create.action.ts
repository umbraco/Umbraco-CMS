import { UmbElementItemRepository } from '../../item/index.js';
import { UMB_ELEMENT_CREATE_OPTIONS_MODAL } from './element-create-options-modal.token.js';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';

export class UmbCreateElementEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		// default to root
		let elementItem = null;

		if (this.args.unique) {
			// get element item to get the element type id
			const itemRepository = new UmbElementItemRepository(this._host);
			const { data, error } = await itemRepository.requestItems([this.args.unique]);
			if (error || !data) throw new Error(`Failed to load element item`);
			elementItem = data[0];
		}

		await umbOpenModal(this, UMB_ELEMENT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: { unique: this.args.unique, entityType: this.args.entityType },
				documentType: elementItem ? { unique: elementItem.documentType.unique } : null,
			},
		});
	}
}
export default UmbCreateElementEntityAction;
