import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import type { ConditionTypes } from '../conditions/types.js';
import type { UmbEntityAction, UmbEntityActionElement } from '@umbraco-cms/backoffice/entity-action';
import type { UmbModalToken, UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

/**
 * An action to perform on an entity
 * For example for content you may wish to create a new document etc
 */
export interface ManifestEntityAction<MetaType extends MetaEntityAction = MetaEntityAction>
	extends ManifestElementAndApi<UmbEntityActionElement, UmbEntityAction<MetaType>>,
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

// RESTORE FROM RECYCLE BIN

export interface ManifestEntityActionRestoreFromRecycleBinKind
	extends ManifestEntityAction<MetaEntityActionRestoreFromRecycleBinKind> {
	type: 'entityAction';
	kind: 'restoreFromRecycleBin';
}

export interface MetaEntityActionRestoreFromRecycleBinKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
	itemRepositoryAlias: string;
	pickerModal: UmbModalToken<UmbPickerModalData<any>, UmbPickerModalValue> | string;
}

// EMPTY RECYCLE BIN
export interface ManifestEntityActionEmptyRecycleBinKind
	extends ManifestEntityAction<MetaEntityActionEmptyRecycleBinKind> {
	type: 'entityAction';
	kind: 'emptyRecycleBin';
}

export interface MetaEntityActionEmptyRecycleBinKind extends MetaEntityActionDefaultKind {
	recycleBinRepositoryAlias: string;
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

// DUPLICATE TO
export interface ManifestEntityActionDuplicateToKind extends ManifestEntityAction<MetaEntityActionDuplicateToKind> {
	type: 'entityAction';
	kind: 'duplicateTo';
}

export interface MetaEntityActionDuplicateToKind extends MetaEntityActionDefaultKind {
	duplicateRepositoryAlias: string;
	treeRepositoryAlias: string;
	treeAlias: string;
	foldersOnly?: boolean;
}

// MOVE TO
export interface ManifestEntityActionMoveToKind extends ManifestEntityAction<MetaEntityActionMoveToKind> {
	type: 'entityAction';
	kind: 'moveTo';
}

export interface MetaEntityActionMoveToKind extends MetaEntityActionDefaultKind {
	moveRepositoryAlias: string;
	treeRepositoryAlias: string;
	treeAlias: string;
	foldersOnly?: boolean;
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

// SORT CHILDREN OF
export interface ManifestEntityActionSortChildrenOfKind
	extends ManifestEntityAction<MetaEntityActionSortChildrenOfKind> {
	type: 'entityAction';
	kind: 'sortChildrenOf';
}

export interface MetaEntityActionSortChildrenOfKind extends MetaEntityActionDefaultKind {
	sortChildrenOfRepositoryAlias: string;
	treeRepositoryAlias: string;
}
