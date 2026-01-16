import type { UmbElementEntityType, UmbElementRootEntityType, UmbElementFolderEntityType } from '../entity.js';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

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
