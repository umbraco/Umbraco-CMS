import { UmbDocumentTreeRepository } from '../../tree/document-tree.repository.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UMB_DOCUMENT_NAVIGATION_STRUCTURE_CONTEXT } from './document-navigation-structure.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbVariantableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';

interface UmbVariantStructureModel {
	unique: string | null;
	entityType: string;
	variants: [
		{
			name: string;
			culture: string | null;
			segment: string | null;
		},
	];
}

export class UmbDocumentNavigationStructureContext extends UmbContextBase<UmbDocumentNavigationStructureContext> {
	#workspaceContext?: UmbVariantableWorkspaceContextInterface<UmbVariantModel>;
	#treeRepository = new UmbDocumentTreeRepository(this);

	#structure = new UmbArrayState<UmbVariantStructureModel>([], (x) => x.unique);
	public readonly structure = this.#structure.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_NAVIGATION_STRUCTURE_CONTEXT);

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAncestors();
		});
	}

	async #requestAncestors() {
		const { data: treeRootData } = await this.#treeRepository.requestTreeRoot();

		/* TODO: implement breadcrumb for new items
		 We currently miss the parent item name for new items. We need to align with backend
		 how to solve it */
		const isNew = this.#workspaceContext?.getIsNew();
		if (isNew === true) return;

		this.observe(this.#workspaceContext?.unique, async (unique) => {
			if (!unique) return;

			const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

			if (data) {
				const structureItems = data.map((item: UmbDocumentTreeItemModel) => {
					return {
						unique: item.unique,
						entityType: item.entityType,
						variants: item.variants.map((variant) => {
							return {
								name: variant.name,
								culture: variant.culture,
								segment: variant.segment,
							};
						}),
					};
				});

				this.#structure.setValue(structureItems);
				console.log(this.#structure.getValue());
			}
		});
	}
}

export default UmbDocumentNavigationStructureContext;
