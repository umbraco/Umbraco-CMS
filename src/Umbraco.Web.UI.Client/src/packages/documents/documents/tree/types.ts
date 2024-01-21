import { UmbDocumentEntityType, UmbDocumentRootEntityType } from '../entity.js';
import { PublishedStateModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbDocumentEntityType;
	noAccess: boolean;
	isTrashed: boolean;
	isProtected: boolean;
	isPublished: boolean;
	isEdited: boolean;
	contentTypeId: string;
	variants: Array<UmbDocumentVariantTreeItemModel>;
	icon: string;
}

export interface UmbDocumentTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbDocumentRootEntityType;
}

export interface UmbDocumentVariantTreeItemModel {
	name: string;
	culture: string | null;
	state: PublishedStateModel; // TODO: make our own enum for this. We might have states for "unsaved changes" etc.
}
