import type { ManifestElement } from './models';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document or move a document etc
 */
export interface ManifestEntityAction extends ManifestElement {
	type: 'entityAction';
	meta: MetaEntityAction;
}

export interface MetaEntityAction {
	/**
	 * An icon to represent the action to be performed
	 * @example 'umb:box'
	 * @example 'umb:grid'
	 */
	icon?: string;

	/**
	 * The friendly name of the action to perform
	 * @example 'Move'
	 * @example 'Create'
	 * @example 'Create Content Template'
	 */
	label: string;
	
	/**
	 * The type of entity this action is for such as 'document'
	 * @example 'media'
	 * @example 'document'
	 */
	entityType: string;

	api: any; // TODO: create interface

	/**
	 * The alias for the repsoitory of the entity type this action is for
	 * such as 'Umb.Repository.Documents'
	 * @example 'Umb.Repository.Documents'
	 */
	repositoryAlias: string;
}
