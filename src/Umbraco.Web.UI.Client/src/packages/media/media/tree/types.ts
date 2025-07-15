import type { UmbMediaEntityType, UmbMediaRootEntityType } from '../entity.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeRootModel,
} from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends UmbTreeItemModel {
	entityType: UmbMediaEntityType;
	noAccess: boolean;
	isTrashed: boolean;
	mediaType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	variants: Array<UmbMediaTreeItemVariantModel>;
	createDate: string;
}

export interface UmbMediaTreeRootModel extends UmbTreeRootModel {
	entityType: UmbMediaRootEntityType;
}

export interface UmbMediaTreeItemVariantModel {
	name: string;
	culture: string | null;
}

export interface UmbMediaTreeRootItemsRequestArgs extends UmbTreeRootItemsRequestArgs {
	dataType?: {
		unique: string;
	};
}

export interface UmbMediaTreeChildrenOfRequestArgs extends UmbTreeChildrenOfRequestArgs {
	dataType?: {
		unique: string;
	};
}
