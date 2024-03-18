import { UmbDocumentTreeRepository } from '../../tree/document-tree.repository.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { UMB_DOCUMENT_NAVIGATION_STRUCTURE_CONTEXT } from './document-navigation-structure.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbVariantableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import { UMB_VARIANT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantModel } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentNavigationStructureContext extends UmbContextBase<UmbDocumentNavigationStructureContext> {
	#workspaceContext?: UmbVariantableWorkspaceContextInterface<UmbVariantModel>;
	#treeRepository = new UmbDocumentTreeRepository(this);

	#ancestors = new UmbArrayState<UmbDocumentTreeItemModel>([], (x) => x.unique);
	public readonly ancestors = this.#ancestors.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_NAVIGATION_STRUCTURE_CONTEXT);

		this.consumeContext(UMB_VARIANT_WORKSPACE_CONTEXT, (instance) => {
			this.#workspaceContext = instance;
			this.#requestAncestors();
		});
	}

	async #requestAncestors() {
		/* TODO: implement breadcrumb for new items
		 We currently miss the parent item name for new items. We need to align with backend
		 how to solve it */
		const isNew = this.#workspaceContext?.getIsNew();
		if (isNew === true) return;

		this.observe(this.#workspaceContext?.unique, async (unique) => {
			if (!unique) return;

			const { data } = await this.#treeRepository.requestTreeItemAncestors({ descendantUnique: unique });

			if (data) {
				this.#ancestors.setValue(data);
			}
		});
	}
}

export default UmbDocumentNavigationStructureContext;
