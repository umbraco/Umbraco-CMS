import type { UmbStructureItemModel } from './types.js';
import type { UmbTreeRepository, UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface UmbMenuTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

export abstract class UmbMenuTreeStructureWorkspaceContextBase extends UmbContextBase {
	#workspaceContext?: typeof UMB_ENTITY_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parent = new UmbObjectState<UmbStructureItemModel | undefined>(undefined);
	public readonly parent = this.#parent.asObservable();

	constructor(host: UmbControllerHost, args: UmbMenuTreeStructureWorkspaceContextBaseArgs) {
		// TODO: set up context token
		super(host, 'UmbMenuStructureWorkspaceContext');
		this.#args = args;

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(this.#workspaceContext?.unique, (value) => {
				if (!value) return;
				this.#requestStructure();
			});
		});
	}

	async #requestStructure() {
		let structureItems: Array<UmbStructureItemModel> = [];

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<UmbTreeItemModel, UmbTreeRootModel>>(
			this,
			this.#args.treeRepositoryAlias,
		);

		const { data: root } = await treeRepository.requestTreeRoot();

		if (root) {
			structureItems = [
				{
					unique: root.unique,
					entityType: root.entityType,
					name: root.name,
					isFolder: root.isFolder,
				},
			];
		}

		const isNew = this.#workspaceContext?.getIsNew();

		const entityTypeObservable = isNew ? this.#workspaceContext?.parentEntityType : this.#workspaceContext?.entityType;
		const entityType = (await this.observe(entityTypeObservable, () => {})?.asPromise()) as string;
		if (!entityType) throw new Error('Entity type is not available');

		// If the entity type is different from the root entity type, then we can request the ancestors.
		if (entityType !== root?.entityType) {
			const uniqueObservable = isNew ? this.#workspaceContext?.parentUnique : this.#workspaceContext?.unique;
			const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
			if (!unique) throw new Error('Unique is not available');

			const { data } = await treeRepository.requestTreeItemAncestors({ treeItem: { unique, entityType } });

			if (data) {
				const ancestorItems = data.map((treeItem) => {
					return {
						unique: treeItem.unique,
						entityType: treeItem.entityType,
						name: treeItem.name,
						isFolder: treeItem.isFolder,
					};
				});

				structureItems.push(...ancestorItems);
			}
		}

		const parent = structureItems[structureItems.length - 2];
		this.#parent.setValue(parent);
		this.#structure.setValue(structureItems);
	}
}
