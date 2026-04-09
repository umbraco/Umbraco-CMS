import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbAllowedMemberTypeModel extends UmbEntityModel {
	name: string;
	description: string | null;
	icon: string | null;
}
