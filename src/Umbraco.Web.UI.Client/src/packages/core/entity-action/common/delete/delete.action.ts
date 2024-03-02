import { UmbEntityActionBase } from '../../entity-action-base.js';
import type { UmbEntityActionArgs } from '../../types.js';
import type { MetaEntityActionDeleteKind } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';

export class UmbDeleteEntityAction extends UmbEntityActionBase<MetaEntityActionDeleteKind> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<MetaEntityActionDeleteKind>) {
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
