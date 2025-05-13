import type { UmbEntityBulkActionElement } from '../../entity-bulk-action/entity-bulk-action-element.interface.js';
import type { UmbEntityBulkAction } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { ManifestElementAndApi, ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';

/**
 * An action to perform on multiple entities
 * For example for content you may wish to move one or more documents in bulk
 */
export interface ManifestEntityBulkAction<MetaType extends MetaEntityBulkAction = MetaEntityBulkAction>
	extends ManifestElementAndApi<UmbEntityBulkActionElement, UmbEntityBulkAction<MetaType>>,
		ManifestWithDynamicConditions<UmbExtensionConditionConfig> {
	type: 'entityBulkAction';
	forEntityTypes: Array<string>;
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaEntityBulkAction {}

export interface ManifestEntityBulkActionDefaultKind<
	MetaKindType extends MetaEntityBulkActionDefaultKind = MetaEntityBulkActionDefaultKind,
> extends ManifestEntityBulkAction<MetaKindType> {
	type: 'entityBulkAction';
	kind: 'default';
}

export interface MetaEntityBulkActionDefaultKind extends MetaEntityBulkAction {
	/**
	 * An icon to represent the action to be performed
	 * @examples [
	 *   "icon-box",
	 *   "icon-grid"
	 * ]
	 */
	icon: string;

	/**
	 * The friendly name of the action to perform
	 * @examples [
	 *   "Create",
	 *   "Create Content Template"
	 * ]
	 */
	label?: string;
}

// DUPLICATE TO
export interface ManifestEntityBulkActionDuplicateToKind
	extends ManifestEntityBulkAction<MetaEntityBulkActionDuplicateToKind> {
	type: 'entityBulkAction';
	kind: 'duplicateTo';
}

export interface MetaEntityBulkActionDuplicateToKind extends ManifestEntityBulkAction {
	bulkDuplicateRepositoryAlias: string;
	hideTreeRoot?: boolean;
	foldersOnly?: boolean;
	treeAlias: string;
}

// MOVE TO
export interface ManifestEntityBulkActionMoveToKind extends ManifestEntityBulkAction<MetaEntityBulkActionMoveToKind> {
	type: 'entityBulkAction';
	kind: 'moveTo';
}

export interface MetaEntityBulkActionMoveToKind extends MetaEntityBulkActionDefaultKind {
	bulkMoveRepositoryAlias: string;
	hideTreeRoot?: boolean;
	foldersOnly?: boolean;
	treeAlias: string;
}

// TRASH
export interface ManifestEntityBulkActionTrashKind extends ManifestEntityBulkAction<MetaEntityBulkActionTrashKind> {
	type: 'entityBulkAction';
	kind: 'trash';
}

export interface MetaEntityBulkActionTrashKind extends MetaEntityBulkActionDefaultKind {
	bulkTrashRepositoryAlias: string;
}

export type UmbEntityBulkActionExtensions =
	| ManifestEntityBulkAction
	| ManifestEntityBulkActionDefaultKind
	| ManifestEntityBulkActionDuplicateToKind
	| ManifestEntityBulkActionMoveToKind
	| ManifestEntityBulkActionTrashKind;

declare global {
	interface UmbExtensionManifestMap {
		UmbEntityBulkActionExtensions: UmbEntityBulkActionExtensions;
	}
}
