import type { UmbVariantStructureItemModel } from './types.js';
import type { UmbTreeRepository } from '@umbraco-cms/backoffice/tree';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

interface UmbMenuVariantTreeStructureWorkspaceContextBaseArgs {
	treeRepositoryAlias: string;
}

export abstract class UmbMenuVariantTreeStructureWorkspaceContextBase extends UmbContextBase<unknown> {
	#workspaceContext?: typeof UMB_VARIANT_WORKSPACE_CONTEXT.TYPE;
	#args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs;

	#structure = new UmbArrayState<UmbVariantStructureItemModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	constructor(host: UmbControllerHost, args: UmbMenuVariantTreeStructureWorkspaceContextBaseArgs) {
		// TODO: set up context token
		super(host, 'UmbMenuStructureWorkspaceContext');
		this.#args = args;

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestStructure();
		});
	}

	async #requestStructure() {
		const isNew = this.#workspaceContext?.getIsNew();
		const uniqueObservable = isNew ? this.#workspaceContext?.parentUnique : this.#workspaceContext?.unique;

		const unique = (await this.observe(uniqueObservable, () => {})?.asPromise()) as string;
		if (!unique) throw new Error('Unique is not available');

		const treeRepository = await createExtensionApiByAlias<UmbTreeRepository<any>>(
			this,
			this.#args.treeRepositoryAlias,
		);
		const { data } = await treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

		if (data) {
			const structureItems = data.map((treeItem) => {
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

			this.#structure.setValue(structureItems);
		}
	}
}
