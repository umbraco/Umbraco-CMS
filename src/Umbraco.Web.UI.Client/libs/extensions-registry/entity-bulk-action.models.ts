import type { ManifestElement } from './models';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction extends ManifestElement {
	type: 'entityBulkAction';
	meta: MetaEntityBulkAction;
}

export interface MetaEntityBulkAction {
	/**
	 * 
	 */
	label: string;

	/**
	 * The friendly name of the action to perform
	 * @example 'Move'
	 */
	entityType: string;

	api: any; // TODO create interface

	/**
	 * The alias for the repsoitory of the entity type this action is for
	 * such as 'Umb.Repository.Documents'
	 * @example 'Umb.Repository.Documents'
	 */
	repositoryAlias: string;
}
