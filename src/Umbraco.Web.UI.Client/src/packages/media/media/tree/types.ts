import type { UmbMediaEntityType, UmbMediaRootEntityType } from '../entity.js';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

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
}

export interface UmbMediaTreeRootModel extends UmbTreeRootModel {
	entityType: UmbMediaRootEntityType;
}

export interface UmbMediaTreeItemVariantModel {
	name: string;
	culture: string | null;
}
