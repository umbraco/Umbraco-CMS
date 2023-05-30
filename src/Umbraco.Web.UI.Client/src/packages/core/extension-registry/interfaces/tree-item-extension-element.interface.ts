import { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbTreeItemExtensionElement extends HTMLElement {
	item?: TreeItemPresentationModel;
}
