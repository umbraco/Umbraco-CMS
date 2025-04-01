import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMemberTypePropertyTypeReferenceModel extends UmbEntityModel {
	alias: string;
	memberType: {
		alias: string;
		icon: string;
		name: string;
	};
	name: string;
}
