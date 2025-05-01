import type { UmbVariantStructureItemModel } from './types.js';
import type { UmbTreeItemModel, UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';

interface UmbMenuVariantTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

export abstract class UmbMenuVariantTreeStructureWorkspaceContextBase extends UmbContextBase {
	// TODO: add correct interface
	#workspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	#parent = new UmbObjectState<UmbVariantStructureItemModel | undefined>(undefined);
	public readonly parent = this.#parent.asObservable();

	#ancestorContext = new UmbAncestorsEntityContext(this);

	constructor(host: UmbControllerHost, args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs) {
		// TODO: set up context token
		super(host, 'UmbMenuStructureWorkspaceContext');
		this.#args = args;

		// TODO: Implement a Context Token that supports parentUnique, parentEntityType, entityType
		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
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
		const isNew = this.#workspaceContext?.getIsNew() ?? false;
		const uniqueObservable = isNew ? (this.#workspaceContext as any)?.parentUnique : this.#workspaceContext?.unique;
		const entityTypeObservable = isNew
			? (this.#workspaceContext as any)?.parentEntityType
			: (this.#workspaceContext as any)?.entityType;

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

			const parent = structureItems[structureItems.length - 2];
			this.#parent.setValue(parent);
			this.#structure.setValue(structureItems);

			this.#setAncestorData(data);
		}
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
