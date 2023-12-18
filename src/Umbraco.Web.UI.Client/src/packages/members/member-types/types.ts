import { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export interface MemberTypeDetails extends UmbEntityTreeItemModel {
	id: string; // TODO: Remove this when the backend is fixed
	alias: string;
	properties: [];
}
