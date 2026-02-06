import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

export interface UmbExtensionItemModel extends UmbItemModel {
	name: string;
	type: string;
	description?: string;
}
