import type { UmbMemberItemVariantModel } from '../../repository/item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { TrackedReferenceMemberTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMemberReferenceModel extends UmbEntityModel {
	/**
	 * @deprecated use name on the variant array instead
	 * @type {(string | null)}
	 * @memberof UmbMemberReferenceModel
	 */
	name?: string | null;
	memberType: TrackedReferenceMemberTypeModel;
	variants: Array<UmbMemberItemVariantModel>;
}
