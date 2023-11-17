import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbMemberDetailModel extends EntityTreeItemResponseModel {
	id: string; // TODO: Remove this when the backend is fixed
}
