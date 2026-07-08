import type { UmbTreeItemModel } from '../types.js';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';
import type { ManifestBase, UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * The base tree item contract — shared by full tree item contexts and card apis —
 * covering item data, selection, active state, path, and entity actions. Children,
 * expansion, and pagination are not included here; they extend this contract in
 * `UmbTreeItemContext`.
 */
export interface UmbTreeItemApi<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	ManifestType extends ManifestBase = ManifestBase,
>
	extends UmbApi, UmbContextMinimal {
	unique?: UmbEntityUnique;
	entityType?: string;
	manifest: ManifestType | undefined;
	readonly treeItem: Observable<TreeItemType | undefined>;
	readonly isSelectable: Observable<boolean>;
	readonly isSelectableContext: Observable<boolean>;
	readonly selectOnly: Observable<boolean>;
	readonly isSelected: Observable<boolean>;
	readonly isActive: Observable<boolean>;
	readonly hasChildren: Observable<boolean>;
	readonly hasActions: Observable<boolean>;
	readonly noAccess: Observable<boolean>;
	readonly path: Observable<string>;
	setTreeItem(item: TreeItemType | undefined): void;
	getTreeItem(): TreeItemType | undefined;
	open(): void;
	select(): void;
	deselect(): void;
	constructPath(pathname: string, entityType: string, unique: string | null): string;
}
