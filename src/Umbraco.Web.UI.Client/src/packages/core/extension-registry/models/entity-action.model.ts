import type { ConditionTypes } from '../conditions/types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityAction } from '@umbraco-cms/backoffice/entity-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction<MetaType extends MetaEntityAction = MetaEntityAction>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbEntityAction<MetaType>>,
		ManifestWithDynamicConditions<ConditionTypes> {
	type: 'entityAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

export interface MetaEntityAction {}

export interface ManifestEntityActionDefaultKind extends ManifestEntityAction<MetaEntityActionDefaultKind> {
	type: 'entityAction';
	kind: 'default';
}

export interface MetaEntityActionDefaultKind extends MetaEntityAction {
	/**
	 * An icon to represent the action to be performed
	 *
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon: string;

	/**
	 * The friendly name of the action to perform
	 *
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label: string;
}

// DELETE
export interface ManifestEntityActionDeleteKind extends ManifestEntityAction<MetaEntityActionDeleteKind> {
	type: 'entityAction';
	kind: 'delete';
}

export interface MetaEntityActionDeleteKind extends MetaEntityActionDefaultKind {
	detailRepositoryAlias: string;
	itemRepositoryAlias: string;
}

// TRASH
export interface ManifestEntityActionTrashKind extends ManifestEntityAction<MetaEntityActionTrashKind> {
	type: 'entityAction';
	kind: 'trash';
}

export interface MetaEntityActionTrashKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
}

// RENAME
export interface ManifestEntityActionRenameServerFileKind
	extends ManifestEntityAction<MetaEntityActionRenameServerFileKind> {
	type: 'entityAction';
	kind: 'renameServerFile';
}

export interface MetaEntityActionRenameServerFileKind extends MetaEntityActionDefaultKind {
	renameRepositoryAlias: string;
	itemRepositoryAlias: string;
}

// RELOAD TREE ITEM CHILDREN
export interface ManifestEntityActionReloadTreeItemChildrenKind
	extends ManifestEntityAction<MetaEntityActionRenameServerFileKind> {
	type: 'entityAction';
	kind: 'reloadTreeItemChildren';
}

export interface MetaEntityActionReloadTreeItemChildrenKind extends MetaEntityActionDefaultKind {}

// DUPLICATE
export interface ManifestEntityActionDuplicateKind extends ManifestEntityAction<MetaEntityActionDuplicateKind> {
	type: 'entityAction';
	kind: 'duplicate';
}

export interface MetaEntityActionDuplicateKind extends MetaEntityActionDefaultKind {
	duplicateRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken | string;
}

// MOVE
export interface ManifestEntityActionMoveKind extends ManifestEntityAction<MetaEntityActionMoveKind> {
	type: 'entityAction';
	kind: 'move';
}

export interface MetaEntityActionMoveKind extends MetaEntityActionDefaultKind {
	moveRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken | string;
}

// FOLDER
export interface ManifestEntityActionCreateFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	type: 'entityAction';
	kind: 'folderCreate';
}

export interface ManifestEntityActionUpdateFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	type: 'entityAction';
	kind: 'folderUpdate';
}

export interface ManifestEntityActionDeleteFolderKind extends ManifestEntityAction<MetaEntityActionFolderKind> {
	type: 'entityAction';
	kind: 'folderDelete';
}

export interface MetaEntityActionFolderKind extends MetaEntityActionDefaultKind {
	folderRepositoryAlias: string;
}
