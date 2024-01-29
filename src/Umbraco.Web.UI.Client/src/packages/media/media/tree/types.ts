import type { UmbMediaEntityType, UmbMediaRootEntityType } from '../entity.js';
import type { ContentStateModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbUniqueTreeItemModel, UmbUniqueTreeRootModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMediaTreeItemModel extends UmbUniqueTreeItemModel {
	entityType: UmbMediaEntityType;
	noAccess: boolean;
	isTrashed: boolean;
	mediaType: {
		id: string;
		icon: string;
		hasListView: boolean;
	};
	variants: Array<UmbMediaTreeItemVariantModel>;
}

export interface UmbMediaTreeRootModel extends UmbUniqueTreeRootModel {
	entityType: UmbMediaRootEntityType;
}

export interface UmbMediaTreeItemVariantModel {
	name: string;
	culture: string | null;
	state: ContentStateModel; // TODO: make our own enum for this. We might have states for "unsaved changes" etc.
}
