import { UmbEntityActionBase } from '../../entity-action-base.js';
import { UMB_SORT_CHILDREN_OF_MODAL } from './modal/index.js';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { MetaEntityActionSortChildrenOfKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadTreeItemChildrenEvent } from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfEntityAction extends UmbEntityActionBase<MetaEntityActionSortChildrenOfKind> {
	async execute() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this._host, UMB_SORT_CHILDREN_OF_MODAL, {
			data: {
				unique: this.args.unique,
				entityType: this.args.entityType,
				sortChildrenOfRepositoryAlias: this.args.meta.sortChildrenOfRepositoryAlias,
				treeRepositoryAlias: this.args.meta.treeRepositoryAlias,
			},
		});

		await modal.onSubmit();

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);

		eventContext.dispatchEvent(
			new UmbRequestReloadTreeItemChildrenEvent({
				unique: this.args.unique,
				entityType: this.args.entityType,
			}),
		);
	}
}

export default UmbSortChildrenOfEntityAction;
