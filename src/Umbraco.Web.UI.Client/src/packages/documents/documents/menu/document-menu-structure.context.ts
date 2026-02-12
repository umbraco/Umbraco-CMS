import { UMB_DOCUMENT_TREE_REPOSITORY_ALIAS } from '../tree/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbMenuVariantTreeStructureWorkspaceContextBase,
	type UmbVariantStructureItemModel,
} from '@umbraco-cms/backoffice/menu';

export class UmbDocumentMenuStructureContext extends UmbMenuVariantTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_DOCUMENT_TREE_REPOSITORY_ALIAS });
	}

	override getItemHref(structureItem: UmbVariantStructureItemModel): string | undefined {
		// The Document menu does not have a root item, so we do not have a href for it.
		if (!structureItem.unique) {
			return `section/${this._sectionContext?.getPathname()}`;
		} else {
			return super.getItemHref(structureItem);
		}
	}
}

export default UmbDocumentMenuStructureContext;
