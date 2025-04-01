import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaTypePropertyTypeReferenceModel extends UmbEntityModel {
	alias: string;
	mediaType: {
		alias: string;
		icon: string;
		name: string;
	};
	name: string;
}
