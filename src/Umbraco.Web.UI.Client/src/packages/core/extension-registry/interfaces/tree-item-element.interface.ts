import { TreeItemPresentationModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbTreeItemElement extends HTMLElement {
	item?: TreeItemPresentationModel;
}
