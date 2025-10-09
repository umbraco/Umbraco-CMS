import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
export interface UmbPropertyEditorDataSourceItemModel extends UmbItemModel {
	description?: string;
	name: string;
}
