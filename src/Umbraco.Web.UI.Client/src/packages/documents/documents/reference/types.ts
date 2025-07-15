import type { UmbDocumentItemVariantModel } from '../item/types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDocumentReferenceModel extends UmbEntityModel {
	/**
	 * @deprecated use unique instead
	 * @type {string}
	 * @memberof UmbDocumentReferenceModel
	 */
	id: string;

	/**
	 * @deprecated use name on the variant array instead
	 * @type {(string | null)}
	 * @memberof UmbDocumentReferenceModel
	 */
	name?: string | null;

	/**
	 * @deprecated use state on variant array instead
	 * @type {boolean}
	 * @memberof UmbDocumentReferenceModel
	 */
	published?: boolean | null;
	documentType: {
		alias: string;
		icon: string;
		name: string;
		unique: string;
	};
	variants: Array<UmbDocumentItemVariantModel>;
}
