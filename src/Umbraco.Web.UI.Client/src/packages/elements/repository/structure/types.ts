import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbAllowedElementTypeModel extends UmbNamedEntityModel {
	description: string | null;
	icon: string | null;
}
