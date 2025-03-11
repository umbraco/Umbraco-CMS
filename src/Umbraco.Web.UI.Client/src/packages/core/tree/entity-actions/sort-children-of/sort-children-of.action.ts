import { UMB_SORT_CHILDREN_OF_MODAL } from './modal/index.js';
import type { MetaEntityActionSortChildrenOfKind } from './types.js';
import { UmbEntityActionBase, UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';

export class UmbSortChildrenOfEntityAction extends UmbEntityActionBase<MetaEntityActionSortChildrenOfKind> {
	override async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this._host, UMB_SORT_CHILDREN_OF_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				sortChildrenOfRepositoryAlias: this.args.meta.sortChildrenOfRepositoryAlias,
				treeRepositoryAlias: this.args.meta.treeRepositoryAlias,
			},
		});

		await modal.onSubmit().catch(() => undefined);

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

		eventContext.dispatchEvent(
			new UmbRequestReloadChildrenOfEntityEvent({
				unique: this.args.unique,
				entityType: this.args.entityType,
			}),
		);
	}
}

export default UmbSortChildrenOfEntityAction;
