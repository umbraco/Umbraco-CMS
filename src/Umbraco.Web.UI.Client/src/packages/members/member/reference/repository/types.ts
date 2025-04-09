import type { UmbMemberItemVariantModel } from '../../item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMemberReferenceModel extends UmbEntityModel {
	/**
	 * @deprecated use name on the variant array instead
	 * @type {(string | null)}
	 * @memberof UmbMemberReferenceModel
	 */
	name?: string | null;
	memberType: {
		alias: string;
		icon: string;
		name: string;
		unique: string;
	};
	variants: Array<UmbMemberItemVariantModel>;
}
