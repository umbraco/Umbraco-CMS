import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbAllowedElementTypeModel extends UmbEntityModel {
	name: string;
	description: string | null;
	icon: string | null;
}
