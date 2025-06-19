import { UmbDocumentItemRepository } from '../../item/index.js';
import { UMB_DOCUMENT_CREATE_OPTIONS_MODAL } from './document-create-options-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		// default to root
		let documentItem = null;

		if (this.args.unique) {
			// get document item to get the doc type id
			const itemRepository = new UmbDocumentItemRepository(this._host);
			const { data, error } = await itemRepository.requestItems([this.args.unique]);
			if (error || !data) throw new Error(`Failed to load document item`);
			documentItem = data[0];
		}

		await umbOpenModal(this, UMB_DOCUMENT_CREATE_OPTIONS_MODAL, {
			data: {
				parent: { unique: this.args.unique, entityType: this.args.entityType },
				documentType: documentItem ? { unique: documentItem.documentType.unique } : null,
			},
		});
	}
}
export default UmbCreateDocumentEntityAction;
