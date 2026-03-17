import type { UmbElementEntityType, UmbElementRootEntityType, UmbElementFolderEntityType } from '../entity.js';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeRootModel,
} from '@umbraco-cms/backoffice/tree';

export type { UmbElementTreeItemContext } from './element-tree-item.context.js';
export type { UmbElementTreeRepository } from './element-tree.repository.js';

export interface UmbElementTreeItemModel extends UmbTreeItemModel {
	entityType: UmbElementEntityType | UmbElementFolderEntityType;
	isTrashed: boolean;
	documentType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	createDate: string;
	variants: Array<UmbElementTreeItemVariantModel>;
}

export interface UmbElementTreeRootModel extends UmbTreeRootModel {
	entityType: UmbElementRootEntityType;
}

export interface UmbElementTreeItemVariantModel {
	name: string;
	culture: string | null;
	segment: string | null;
	state: DocumentVariantStateModel;
}

export interface UmbElementTreeRootItemsRequestArgs extends UmbTreeRootItemsRequestArgs {
	dataType?: {
		unique: string;
	};
}

export interface UmbElementTreeChildrenOfRequestArgs extends UmbTreeChildrenOfRequestArgs {
	dataType?: {
		unique: string;
	};
}
