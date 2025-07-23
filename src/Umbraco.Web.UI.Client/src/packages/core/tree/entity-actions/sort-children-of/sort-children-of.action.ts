import { UMB_SORT_CHILDREN_OF_MODAL } from './modal/index.js';
import type { MetaEntityActionSortChildrenOfKind } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbSortChildrenOfEntityAction extends UmbEntityActionBase<MetaEntityActionSortChildrenOfKind> {
	override async execute() {
		await umbOpenModal(this, this._getModalToken(), {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				sortChildrenOfRepositoryAlias: this.args.meta.sortChildrenOfRepositoryAlias,
				treeRepositoryAlias: this.args.meta.treeRepositoryAlias,
			},
		}).catch(() => undefined);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) throw new Error('Event context is not available');

		eventContext.dispatchEvent(
			new UmbRequestReloadChildrenOfEntityEvent({
				unique: this.args.unique,
				entityType: this.args.entityType,
			}),
		);
	}

	protected _getModalToken() {
		return UMB_SORT_CHILDREN_OF_MODAL;
	}
}

export default UmbSortChildrenOfEntityAction;
