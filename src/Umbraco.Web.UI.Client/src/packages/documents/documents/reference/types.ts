import type { UmbDocumentItemVariantModel } from '../item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentReferenceModel extends UmbEntityModel {
	documentType: {
		alias: string;
		icon: string;
		name: string;
		unique: string;
	};
	variants: Array<UmbDocumentItemVariantModel>;
}
