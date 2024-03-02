import type { UmbMediaDetailRepository } from '../../repository/index.js';
import { UmbMediaItemRepository } from '../../repository/index.js';
import type { UmbMediaCreateOptionsModalData } from './media-create-options-modal.token.js';
import { UMB_MEDIA_CREATE_OPTIONS_MODAL } from './media-create-options-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateMediaEntityAction extends UmbEntityActionBase<UmbMediaDetailRepository> {
	#itemRepository;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.#itemRepository = new UmbMediaItemRepository(host);
	}

	async execute() {
		if (!this.repository) return;

		// default to root
		let mediaItem = null;

		if (this.unique) {
			// get media item to get the doc type id
			const { data, error } = await this.#itemRepository.requestItems([this.unique]);
			if (error || !data) throw new Error(`Failed to load media item`);
			mediaItem = data[0];
		}

		this._openModal({
			media: mediaItem ? { unique: mediaItem.unique } : null,
			mediaType: mediaItem ? { unique: mediaItem.mediaType.unique } : null,
		});
	}

	private async _openModal(modalData: UmbMediaCreateOptionsModalData) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager.open(this, UMB_MEDIA_CREATE_OPTIONS_MODAL, {
			data: modalData,
		});
	}
}
