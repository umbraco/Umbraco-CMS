import type { UmbVariantStructureItemModel } from './types.js';
import { UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT } from './menu-variant-structure-workspace-context.context-token.js';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VARIANT_TREE_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext, UmbParentEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';

interface UmbMenuVariantTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

export abstract class UmbMenuVariantTreeStructureWorkspaceContextBase extends UmbContextBase {
	//
	#workspaceContext?: typeof UMB_VARIANT_TREE_ENTITY_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parent = new UmbObjectState<UmbVariantStructureItemModel | undefined>(undefined);
	public readonly parent = this.#parent.asObservable();

	#parentContext = new UmbParentEntityContext(this);
	#ancestorContext = new UmbAncestorsEntityContext(this);

	public readonly IS_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT = true;

	constructor(host: UmbControllerHost, args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs) {
		super(host, UMB_MENU_VARIANT_STRUCTURE_WORKSPACE_CONTEXT);
		// 'UmbMenuStructureWorkspaceContext' is Obsolete, will be removed in v.18
		this.provideContext('UmbMenuStructureWorkspaceContext', this);
		this.#args = args;

		this.consumeContext(UMB_VARIANT_TREE_ENTITY_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.observe(
				this.#workspaceContext?.unique,
				(value) => {
					if (!value) return;
					this.#requestStructure();
				},
				'observeUnique',
			);
		});
	}

	async #requestStructure() {
		const isNew = this.#workspaceContext?.getIsNew();
		const uniqueObservable = isNew ? this.#workspaceContext?.parentUnique : this.#workspaceContext?.unique;
		const entityTypeObservable = isNew ? this.#workspaceContext?.parentEntityType : this.#workspaceContext?.entityType;

		let structureItems: Array<UmbVariantStructureItemModel> = [];

		const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
		if (unique === undefined) throw new Error('Unique is not available');

		const entityType = (await this.observe(entityTypeObservable, () => {})?.asPromise()) as string;
		if (!entityType) throw new Error('Entity type is not available');

		// TODO: introduce variant tree item model
		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<any, UmbTreeRootModel>>(
			this,
			this.#args.treeRepositoryAlias,
		);

		const { data: root } = await treeRepository.requestTreeRoot();

		if (root) {
			structureItems = [
				{
					unique: root.unique,
					entityType: root.entityType,
					variants: [{ name: root.name, culture: null, segment: null }],
				},
			];
		}

		const { data } = await treeRepository.requestTreeItemAncestors({ treeItem: { unique, entityType } });

		if (data) {
			const treeItemAncestors = data.map((treeItem) => {
				return {
					unique: treeItem.unique,
					entityType: treeItem.entityType,
					variants: treeItem.variants.map((variant: any) => {
						return {
							name: variant.name,
							culture: variant.culture,
							segment: variant.segment,
						};
					}),
				};
			});

			structureItems.push(...treeItemAncestors);

			this.#structure.setValue(structureItems);

			this.#handleParent(structureItems);
			this.#setAncestorData(data);
		}
	}

	#handleParent(structureItems: Array<UmbVariantStructureItemModel>) {
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

	#setAncestorData(data: Array<UmbTreeItemModel>) {
		const ancestorEntities = data
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
