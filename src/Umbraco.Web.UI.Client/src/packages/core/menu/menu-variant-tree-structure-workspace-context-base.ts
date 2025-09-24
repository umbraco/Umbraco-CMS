import type { UmbVariantStructureItemModel } from './types.js';
import type { UmbTreeRepository, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbAncestorsEntityContext } from '@umbraco-cms/backoffice/entity';

interface UmbMenuVariantTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

export abstract class UmbMenuVariantTreeStructureWorkspaceContextBase extends UmbContextBase<unknown> {
	// TODO: add correct interface
	#workspaceContext?: any;
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

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-ignore
			this.#workspaceContext.observe(this.#workspaceContext.unique, (value) => {
				if (!value) return;
				this.#requestStructure();
			});
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

		// TODO: add correct tree variant item model
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
			const ancestorItems = data.map((treeItem) => {
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

			const ancestorEntities = data.map((treeItem) => {
				return {
					unique: treeItem.unique,
					entityType: treeItem.entityType,
				};
			});

			this.#ancestorContext.setAncestors(ancestorEntities);

			structureItems.push(...ancestorItems);

			const parent = structureItems[structureItems.length - 2];
			this.#parent.setValue(parent);
			this.#structure.setValue(structureItems);
		}
	}
}
