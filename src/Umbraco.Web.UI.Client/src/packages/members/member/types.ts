import type { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export interface UmbMemberDetailModel extends UmbEntityTreeItemModel {
	id: string; // TODO: Remove this when the backend is fixed
}
