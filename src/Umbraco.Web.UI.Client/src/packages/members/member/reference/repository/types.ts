import type { UmbMemberItemVariantModel } from '../../repository/item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { TrackedReferenceMemberTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export interface UmbMemberReferenceModel extends UmbEntityModel {
	/**
	 * @deprecated use unique instead
	 * @type {string}
	 * @memberof UmbMemberReferenceModel
	 */
	id: string;

	/**
	 * @deprecated use name on the variant array instead
	 * @type {(string | null)}
	 * @memberof UmbMemberReferenceModel
	 */
	name?: string | null;

	/**
	 * @deprecated use state on variant array instead
	 * @type {boolean}
	 * @memberof UmbMemberReferenceModel
	 */
	published?: boolean | null;
	memberType: TrackedReferenceMemberTypeModel;
	variants: Array<UmbMemberItemVariantModel>;
}
