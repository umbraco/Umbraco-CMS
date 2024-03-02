import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
// TODO: create interface for API
export interface ManifestEntityAction extends ManifestElementAndApi, ManifestWithDynamicConditions {
	type: 'entityAction';
	meta: MetaEntityAction;
}

export interface MetaEntityAction {
	/**
	 * An icon to represent the action to be performed
	 *
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon?: string;

	/**
	 * The friendly name of the action to perform
	 *
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label: string;

	entityTypes: Array<string>;
}

// TODO: Warren these don't seem to be used anywhere

export interface ManifestEntityActionDeleteKind extends ManifestEntityAction {
	type: 'entityAction';
	kind: 'delete';
	meta: MetaEntityActionDeleteKind;
}

export interface MetaEntityActionDeleteKind extends MetaEntityAction {
	detailRepositoryAlias: string;
	itemRepositoryAlias: string;
}
