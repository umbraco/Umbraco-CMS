import type { UmbElementItemVariantModel } from '../item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbElementReferenceModel extends UmbEntityModel {
	documentType: {
		alias: string;
		icon: string;
		name: string;
		unique: string;
	};
	variants: Array<UmbElementItemVariantModel>;
}
