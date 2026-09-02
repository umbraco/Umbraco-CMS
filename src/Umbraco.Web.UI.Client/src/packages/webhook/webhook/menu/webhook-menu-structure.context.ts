import { UMB_WEBHOOK_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuStructureWorkspaceContextBase, type UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export class UmbWebhookMenuStructureWorkspaceContext extends UmbMenuStructureWorkspaceContextBase {
	#workspaceContext?: typeof UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			if (!instance) return;

			this.observe(
				observeMultiple([instance.unique, instance.entityType, instance.name]),
				([unique, entityType, name]) => this.#requestStructure(unique, entityType, name),
				'umbWebhookMenuStructureObserver',
			);
		});
	}

	#requestStructure(unique: string | null | undefined, entityType: string | undefined, name: string | undefined) {
		if (!entityType) return;

		// While new, the item itself does not exist yet, so its ancestors are just the (fixed) root.
		const items: Array<UmbStructureItemModel> = [
			{
				unique: null,
				entityType: UMB_WEBHOOK_ROOT_ENTITY_TYPE,
				name: '#treeHeaders_webhooks',
				isFolder: false,
			},
		];

		if (!this.#workspaceContext?.getIsNew()) {
			items.push({
				unique: unique ?? null,
				entityType,
				name: name ?? '',
				isFolder: false,
			});
		}

		this._setStructure(items);
	}
}

export { UmbWebhookMenuStructureWorkspaceContext as api };
