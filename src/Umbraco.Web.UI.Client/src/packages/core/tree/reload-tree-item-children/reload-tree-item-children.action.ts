import type { UmbCopyDataTypeRepository } from '../../../data-type/repository/copy/data-type-copy.repository.js';
import { UmbReloadTreeItemChildrenRequestEntityActionEvent } from './reload-tree-item-children-request.event.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_ACTION_EVENT_CONTEXT, type UmbActionEventContext } from '@umbraco-cms/backoffice/action';

export class UmbReloadTreeItemChildrenEntityAction extends UmbEntityActionBase<UmbCopyDataTypeRepository> {
	#actionEventContext?: UmbActionEventContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (instance) => {
			this.#actionEventContext = instance;
		});
	}

	async execute() {
		if (!this.#actionEventContext) throw new Error('Action Event context is not available');
		this.#actionEventContext.dispatchEvent(
			new UmbReloadTreeItemChildrenRequestEntityActionEvent({
				unique: this.unique,
				entityType: this.entityType,
			}),
		);
	}
}
