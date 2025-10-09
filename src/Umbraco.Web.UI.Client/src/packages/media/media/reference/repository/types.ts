import type { UmbMediaItemVariantModel } from '../../repository/item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbMediaReferenceModel extends UmbEntityModel {
	/**
	 * @deprecated use unique instead
	 * @type {string}
	 * @memberof UmbMediaReferenceModel
	 */
	id: string;

	/**
	 * @deprecated use name on the variant array instead
	 * @type {(string | null)}
	 * @memberof UmbMediaReferenceModel
	 */
	name?: string | null;

	/**
	 * @deprecated use state on variant array instead
	 * @type {boolean}
	 * @memberof UmbMediaReferenceModel
	 */
	published?: boolean | null;
	mediaType: {
		alias: string;
		icon: string;
		name: string;
		unique: string;
	};
	variants: Array<UmbMediaItemVariantModel>;
}
