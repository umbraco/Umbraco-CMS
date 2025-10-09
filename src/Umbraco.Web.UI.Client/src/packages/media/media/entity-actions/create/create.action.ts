import { UmbMediaItemRepository } from '../../repository/index.js';
import { UMB_MEDIA_CREATE_OPTIONS_MODAL } from './media-create-options-modal.token.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';

export class UmbCreateMediaEntityAction extends UmbEntityActionBase<never> {
	    #localization = new UmbLocalizationController(this);

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

		await umbOpenModal(this, UMB_MEDIA_CREATE_OPTIONS_MODAL, {
			data: {
				parent: { unique: this.args.unique, entityType: this.args.entityType },
				mediaType: mediaItem ? { unique: mediaItem.mediaType.unique } : null,
			},
		});
	}
	getCreateAriaLabel(entityName?: string) {
        return this.#localization.term('buttons_createFor', [entityName ?? '']);
    }
    
    getActionsAriaLabel(entityName?: string) {
        return this.#localization.term('buttons_viewActionsFor', [entityName ?? '']);
    }
}

export { UmbCreateMediaEntityAction as api };
