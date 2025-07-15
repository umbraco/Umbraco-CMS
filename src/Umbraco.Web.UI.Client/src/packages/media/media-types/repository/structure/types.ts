import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbAllowedMediaTypeModel extends UmbEntityModel {
	name: string;
	description: string | null;
	icon: string | null;
}
