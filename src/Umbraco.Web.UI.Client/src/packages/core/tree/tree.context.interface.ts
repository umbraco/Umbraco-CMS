import type { ManifestTree, UmbTreeItemModel, UmbTreeRootModel, UmbTreeStartNode } from './types.js';
import type { UmbTreeExpansionManager } from './expansion-manager/index.js';
import type { UmbTreeExpansionModel } from './expansion-manager/types.js';
import type { UmbTreeItemActiveManager } from './active-manager/tree-active-manager.js';
import type { UmbTreeRepository } from './data/tree-repository.interface.js';
import type { UmbTreeRootItemsRequestArgs } from './data/types.js';
import type { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';
import type {
	UmbPaginationManager,
	UmbSelectionManager,
	UmbTargetPaginationManager,
} from '@umbraco-cms/backoffice/utils';

export interface UmbTreeContext<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
	RequestArgsType extends UmbTreeRootItemsRequestArgs = UmbTreeRootItemsRequestArgs,
> extends UmbContextBase {
	manifest: ManifestTree | undefined;

	readonly activeManager: UmbTreeItemActiveManager;

	readonly treeRoot: Observable<TreeRootType | undefined>;
	readonly hideTreeRoot: Observable<boolean | undefined>;
	readonly expandTreeRoot: Observable<boolean | undefined>;

	selectableFilter?(item: TreeItemType): boolean;
	filter?(item: TreeItemType): boolean;

	readonly selection: UmbSelectionManager;
	readonly expansion: UmbTreeExpansionManager;

	readonly rootItems: Observable<TreeItemType[]>;
	readonly hasChildren: Observable<boolean>;
	readonly pagination: UmbPaginationManager;
	readonly targetPagination: UmbTargetPaginationManager;
	readonly startNode: Observable<UmbTreeStartNode | undefined>;
	readonly foldersOnly: Observable<boolean>;
	readonly additionalRequestArgs: Observable<Partial<RequestArgsType> | object>;
	readonly isLoadingPrevChildren: Observable<boolean>;
	readonly isLoadingNextChildren: Observable<boolean>;

	getRepository(): UmbTreeRepository | undefined;

	loadTree(): void;
	reloadTree(): void;
	loadMore(): void;
	loadPrevItems(): void;
	loadNextItems(): void;

	setHideTreeRoot(hideTreeRoot: boolean): void;
	getHideTreeRoot(): boolean;

	setStartNode(startNode: UmbTreeStartNode | undefined): void;
	getStartNode(): UmbTreeStartNode | undefined;

	setFoldersOnly(foldersOnly: boolean): void;
	getFoldersOnly(): boolean;

	updateAdditionalRequestArgs(args: Partial<RequestArgsType>): void;
	getAdditionalRequestArgs(): Partial<RequestArgsType> | undefined;

	setExpansion(data: UmbTreeExpansionModel): void;
	getExpansion(): UmbTreeExpansionModel;

	setExpandTreeRoot(expandTreeRoot: boolean): void;
	getExpandTreeRoot(): boolean | undefined;
}
