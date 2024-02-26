import type { UmbDocumentEntityType, UmbDocumentRootEntityType } from '../entity.js';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';
import type { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbReferenceById } from '@umbraco-cms/backoffice/models';

export interface UmbDocumentTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentEntityType;
	noAccess: boolean;
	isTrashed: boolean;
	isProtected: boolean;
	documentType: {
		unique: string;
		icon: string;
		collection?: UmbReferenceById;
	};
	variants: Array<UmbDocumentTreeItemVariantModel>;
}

export interface UmbDocumentTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentRootEntityType;
}

export interface UmbDocumentTreeItemVariantModel {
	name: string;
	culture: string | null;
	state: DocumentVariantStateModel; // TODO: make our own enum for this. We might have states for "unsaved changes" etc.
}
