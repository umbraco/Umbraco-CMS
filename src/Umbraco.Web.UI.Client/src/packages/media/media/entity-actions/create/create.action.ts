import { UmbMediaItemRepository } from '../../repository/index.js';
import type { UmbMediaCreateOptionsModalData } from './media-create-options-modal.token.js';
import { UMB_MEDIA_CREATE_OPTIONS_MODAL } from './media-create-options-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateMediaEntityAction extends UmbEntityActionBase<UmbEntityActionArgs<never>> {
	#itemRepository;

	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
		this.#itemRepository = new UmbMediaItemRepository(host);
	}

	async execute() {
		// default to root
		let mediaItem = null;

		if (this.args.unique) {
			// get media item to get the doc type id
			const { data, error } = await this.#itemRepository.requestItems([this.args.unique]);
			if (error || !data) throw new Error(`Failed to load media item`);
			mediaItem = data[0];
		}

		if (!mediaItem) throw new Error(`Failed to load media item`);

		this._openModal({
			parent: { unique: this.args.unique, entityType: this.args.entityType },
			mediaType: { unique: mediaItem.mediaType.unique },
		});
	}

	private async _openModal(modalData: UmbMediaCreateOptionsModalData) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_MEDIA_CREATE_OPTIONS_MODAL, {
			data: modalData,
		});

		await modalContext.onSubmit();
	}

	destroy(): void {}
}
