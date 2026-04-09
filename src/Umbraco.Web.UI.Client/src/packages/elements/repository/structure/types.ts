import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Model representing an element type that is allowed for creation.
 * @interface UmbAllowedElementTypeModel
 * @augments {UmbNamedEntityModel}
 */
export interface UmbAllowedElementTypeModel extends UmbNamedEntityModel {
	description: string | null;
	icon: string | null;
}
