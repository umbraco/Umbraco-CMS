import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentTypePropertyTypeReferenceModel extends UmbEntityModel {
	alias: string;
	documentType: {
		alias: string;
		icon: string;
		name: string;
	};
	name: string;
}
