import type { PublishedStateModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbEntityTreeItemModel, UmbEntityTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbDocumentTreeItemModel extends UmbEntityTreeItemModel {
	isProtected: boolean;
	isPublished: boolean;
	isEdited: boolean;
	contentTypeId: string;
	variants: Array<UmbDocumentVariantTreeItemModel>;
	icon: string;
}

export interface UmbDocumentVariantTreeItemModel {
	name: string;
	culture: string | null;
	state: PublishedStateModel; // TODO: make our own enum for this. We might have states for "unsaved changes" etc.
}

// TODO: TREE STORE TYPE PROBLEM:
export interface UmbDocumentTreeRootModel extends UmbEntityTreeRootModel {}
