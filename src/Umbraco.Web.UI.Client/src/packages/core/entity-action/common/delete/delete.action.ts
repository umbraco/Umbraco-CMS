import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbDeleteEntityAction<
	DetailMetaArgs extends UmbEntityActionArgs<DetailMetaArgs>,
> extends UmbEntityActionBase<DetailMetaArgs> {
	constructor(host: UmbControllerHost, args: DetailMetaArgs) {
		super(host, args);
	}

	async execute() {
		await umbConfirmModal(this._host, {
			headline: `Delete`,
			content: 'Are you sure you want to delete this item?',
			color: 'danger',
			confirmLabel: 'Delete',
		});
	}
}
