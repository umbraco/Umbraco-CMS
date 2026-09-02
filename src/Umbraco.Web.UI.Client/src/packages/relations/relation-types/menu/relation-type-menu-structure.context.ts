import { UMB_RELATION_TYPE_WORKSPACE_CONTEXT } from '../workspace/relation-type/relation-type-workspace.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbMenuStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

export class UmbRelationTypeMenuStructureWorkspaceContext extends UmbMenuStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_RELATION_TYPE_WORKSPACE_CONTEXT, (instance) => {
			if (!instance) return;

			this.observe(
				observeMultiple([instance.unique, instance.entityType, instance.name]),
				([unique, entityType, name]) => this.#requestStructure(unique, entityType, name),
				'umbRelationTypeMenuStructureObserver',
			);
		});
	}

	#requestStructure(unique: string | undefined, entityType: string | undefined, name: string | undefined) {
		if (!entityType) return;

		this._setStructure([
			{
				unique: null,
				entityType: 'relations-root',
				name: '#treeHeaders_relations',
				isFolder: false,
			},
			{
				unique: unique ?? null,
				entityType,
				name: name ?? '',
				isFolder: false,
			},
		]);
	}
}

export { UmbRelationTypeMenuStructureWorkspaceContext as api };
