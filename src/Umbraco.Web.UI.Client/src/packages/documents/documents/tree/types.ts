import type { UmbDocumentEntityType, UmbDocumentRootEntityType } from '../entity.js';
import type {
	UmbTreeChildrenOfRequestArgs,
	UmbTreeItemModel,
	UmbTreeRootItemsRequestArgs,
	UmbTreeRootModel,
} from '@umbraco-cms/backoffice/tree';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentTreeItemModel extends UmbTreeItemModel {
	entityType: UmbDocumentEntityType;
	noAccess: boolean;
	isTrashed: boolean;
	isProtected: boolean;
	documentType: {
		unique: string;
		icon: string;
		collection: UmbReferenceByUnique | null;
	};
	createDate: string;
	variants: Array<UmbDocumentTreeItemVariantModel>;
}

export interface UmbDocumentTreeRootModel extends UmbTreeRootModel {
	entityType: UmbDocumentRootEntityType;
}

export interface UmbDocumentTreeItemVariantModel {
	name: string;
	culture: string | null;
	segment: string | null;
	state: DocumentVariantStateModel | null; // TODO: make our own enum for this. We might have states for "unsaved changes" etc.
}

export interface UmbDocumentTreeRootItemsRequestArgs extends UmbTreeRootItemsRequestArgs {
	dataType?: {
		unique: string;
	};
}

export interface UmbDocumentTreeChildrenOfRequestArgs extends UmbTreeChildrenOfRequestArgs {
	dataType?: {
		unique: string;
	};
}
