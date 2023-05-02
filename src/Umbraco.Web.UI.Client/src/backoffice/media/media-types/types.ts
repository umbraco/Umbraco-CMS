import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface MediaTypeDetails extends FolderTreeItemResponseModel {
	id: string; // TODO: Remove this when the backend is fixed
	alias: string;
	properties: [];
}
