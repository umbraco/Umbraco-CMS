import type { UmbStructureItemModel } from './types.js';
import { UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT } from './menu-structure-workspace-context.context-token.js';
import type { UmbTreeRepository, UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext, UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';

interface UmbMenuTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

// TODO: introduce base class for all menu structure workspaces to handle ancestors and parent
export abstract class UmbMenuTreeStructureWorkspaceContextBase extends UmbContextBase {
	#workspaceContext?: typeof UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parent = new UmbObjectState<UmbStructureItemModel | undefined>(undefined);
	/**
	 * @deprecated Will be removed in v.18: Use UMB_PARENT_ENTITY_CONTEXT instead.
	 */
	public readonly parent = this.#parent.asObservable();

	#parentContext = new UmbParentEntityContext(this);
	#ancestorContext = new UmbAncestorsEntityContext(this);

	constructor(host: UmbControllerHost, args: UmbMenuTreeStructureWorkspaceContextBaseArgs) {
		super(host, UMB_MENU_STRUCTURE_WORKSPACE_CONTEXT);
		// 'UmbMenuStructureWorkspaceContext' is Obsolete, will be removed in v.18
		this.provideContext('UmbMenuStructureWorkspaceContext', this);
		this.#args = args;

		this.consumeContext(UMB_SUBMITTABLE_TREE_ENTITY_WORKSPACE_CONTEXT, (instance) => {
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

		const entityTypeObservable = isNew
			? this.#workspaceContext?._internal_createUnderParentEntityType
			: this.#workspaceContext?.entityType;
		const entityType = (await this.observe(entityTypeObservable, () => {})?.asPromise()) as string;
		if (!entityType) throw new Error('Entity type is not available');

		// If the entity type is different from the root entity type, then we can request the ancestors.
		if (entityType !== root?.entityType) {
			const uniqueObservable = isNew
				? this.#workspaceContext?._internal_createUnderParentEntityUnique
				: this.#workspaceContext?.unique;
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

				this.#structure.setValue(structureItems);
				this.#setParentData(structureItems);
				this.#setAncestorData(data);
			}
		}
	}

	#setParentData(structureItems: Array<UmbStructureItemModel>) {
		/* If the item is not new, the current item is the last item in the array. 
			We filter out the current item unique to handle any case where it could show up */
		const parent = structureItems.filter((item) => item.unique !== this.#workspaceContext?.getUnique()).pop();

		// TODO: remove this when the parent gets removed from the structure interface
		this.#parent.setValue(parent);

		const parentEntity = parent
			? {
					unique: parent.unique,
					entityType: parent.entityType,
				}
			: undefined;

		this.#parentContext.setParent(parentEntity);
	}

	/* Notice: ancestors are based on the server "data" ancestors and are not based on the full Menu (UI) structure.
		This will mean that any item placed in the data root will not have any ancestors. But will have a parent based on the UI structure.
	*/
	#setAncestorData(ancestors: Array<UmbTreeItemModel>) {
		const ancestorEntities = ancestors
			.map((treeItem) => {
				const entity: UmbEntityModel = {
					unique: treeItem.unique,
					entityType: treeItem.entityType,
				};

				return entity;
			})
			/* If the item is not new, the current item is the last item in the array. 
				We filter out the current item unique to handle any case where it could show up */
			.filter((item) => item.unique !== this.#workspaceContext?.getUnique());

		this.#ancestorContext.setAncestors(ancestorEntities);
	}
}
