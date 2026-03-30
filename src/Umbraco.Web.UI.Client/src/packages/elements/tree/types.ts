import type { UmbElementEntityType, UmbElementRootEntityType, UmbElementFolderEntityType } from '../entity.js';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbEntityFlag, UmbEntityWithFlags } from '@umbraco-cms/backoffice/entity-flag';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbTreeItemModel, UmbTreeRootModel } from '@umbraco-cms/backoffice/tree';

export type { UmbElementTreeItemContext } from './element-tree-item.context.js';
export type { UmbElementTreeRepository } from './element-tree.repository.js';

export interface UmbElementTreeItemModel extends Omit<UmbTreeItemModel, 'flags'>, UmbEntityWithFlags {
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
	flags: Array<UmbEntityFlag>;
}
