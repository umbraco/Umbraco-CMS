import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

/**
 * Model representing a document type that a document blueprint can be created for.
 * @interface UmbAllowedBlueprintDocumentTypeModel
 * @augments {UmbNamedEntityModel}
 */
export interface UmbAllowedBlueprintDocumentTypeModel extends UmbNamedEntityModel {
	description: string | null;
	icon: string | null;
}
