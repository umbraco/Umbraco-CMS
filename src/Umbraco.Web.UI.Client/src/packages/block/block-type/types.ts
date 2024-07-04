import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/extension-registry';

export interface UmbBlockTypeGroup {
	name?: string;
	key: string;
}

export interface UmbBlockTypeWithGroupKey extends UmbBlockTypeBaseModel {
	groupKey?: string | null;
}
