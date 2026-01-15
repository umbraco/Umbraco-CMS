import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type * from './sort-children-of-content/types.js';

export interface UmbContentTreeItemModel extends UmbTreeItemModel {
	ancestors: Array<UmbEntityModel>;
	entityType: string;
	createDate: string;
}
