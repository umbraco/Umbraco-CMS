import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_MEDIA_CREATE_OPTIONS_MODAL } from './media-create-options-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateMediaEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		// default to root
		let mediaItem = null;

		if (this.args.unique) {
			// get media item to get the doc type id
			const itemRepository = new UmbMediaItemRepository(this._host);
			const { data, error } = await itemRepository.requestItems([this.args.unique]);
			if (error || !data) throw new Error(`Failed to load media item`);
			mediaItem = data[0];
		}

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalContext = modalManager.open(this, UMB_MEDIA_CREATE_OPTIONS_MODAL, {
			data: {
				parent: { unique: this.args.unique, entityType: this.args.entityType },
				mediaType: mediaItem ? { unique: mediaItem.mediaType.unique } : null,
			},
		});

		await modalContext.onSubmit();
	}
}

export { UmbCreateMediaEntityAction as api };
