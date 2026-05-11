import { UMB_ELEMENT_TREE_REPOSITORY_ALIAS } from '../../tree/constants.js';
import { UmbMenuTreeStructureWorkspaceContextBase } from '@umbraco-cms/backoffice/menu';
import type { UmbStructureItemModel } from '@umbraco-cms/backoffice/menu';
import { UMB_WORKSPACE_EDIT_PATH_PATTERN } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbElementFolderMenuStructureContext extends UmbMenuTreeStructureWorkspaceContextBase {
	constructor(host: UmbControllerHost) {
		super(host, { treeRepositoryAlias: UMB_ELEMENT_TREE_REPOSITORY_ALIAS });
	}

	override getItemHref(structureItem: UmbStructureItemModel): string | undefined {
		const sectionName = this._sectionContext?.getPathname();
		if (!sectionName) return undefined;

		if (!structureItem.unique) {
			return `section/${sectionName}`;
		}

		return UMB_WORKSPACE_EDIT_PATH_PATTERN.generateAbsolute({
			sectionName,
			entityType: structureItem.entityType,
			unique: structureItem.unique,
		});
	}
}

export default UmbElementFolderMenuStructureContext;
