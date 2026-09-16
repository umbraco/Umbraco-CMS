import { UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuListStructureWorkspaceContextBase, type UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export class UmbUserGroupMenuStructureWorkspaceContext extends UmbMenuListStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;

			this.observe(
				observeMultiple([instance.unique, instance.entityType, instance.name, instance.isNew]),
				([unique, entityType, name, isNew]) => this.#requestStructure(unique, entityType, name, isNew),
				'umbUserGroupMenuStructureObserver',
			);
		});
	}

	#requestStructure(
		unique: string | null | undefined,
		entityType: string | undefined,
		name: string | undefined,
		isNew: boolean | undefined,
	) {
		if (!entityType) return;

		// While new, the item itself does not exist yet, so its ancestors are just the (fixed) root.
		const items: Array<UmbStructureItemModel> = [
			{
				unique: null,
				entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE,
				name: '#user_usergroups',
				isFolder: false,
			},
		];

		if (!isNew) {
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

export { UmbUserGroupMenuStructureWorkspaceContext as api };
