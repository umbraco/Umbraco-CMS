import type { UmbTreeContext } from '../tree.context.interface.js';
import type { UmbTreeItemModel, UmbTreeRootModel } from '../types.js';
import { UMB_TREE_CONTEXT } from '../tree.context.token.js';
import { state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbTreeViewElementBase<
	TreeItemType extends UmbTreeItemModel = UmbTreeItemModel,
	TreeRootType extends UmbTreeRootModel = UmbTreeRootModel,
> extends UmbLitElement {
	protected _treeContext?: UmbTreeContext<TreeItemType, TreeRootType>;

	@state()
	protected _treeRoot?: TreeRootType;

	@state()
	protected _selectable = false;

	@state()
	protected _selectOnly = false;

	@state()
	protected _selection: Array<string | null> = [];

	constructor() {
		super();
		this.consumeContext(UMB_TREE_CONTEXT, (context) => {
			this._treeContext = context as UmbTreeContext<TreeItemType, TreeRootType>;
			this._observeContext();
		});
	}

	protected _observeContext() {
		this.observe(
			this._treeContext?.treeRoot,
			(treeRoot) => (this._treeRoot = treeRoot as TreeRootType | undefined),
			'_observeTreeRoot',
		);
		this.observe(
			this._treeContext?.selection.selectable,
			(selectable) => (this._selectable = selectable ?? false),
			'_observeSelectable',
		);
		this.observe(
			this._treeContext?.selectOnly,
			(selectOnly) => (this._selectOnly = selectOnly ?? false),
			'_observeSelectOnly',
		);
		this.observe(
			this._treeContext?.selection.selection,
			(selection) => (this._selection = selection ?? []),
			'_observeSelection',
		);
	}

	protected _isSelectableItem(item: TreeItemType): boolean {
		if (!this._selectable) return false;
		return this._treeContext?.selectableFilter?.(item) ?? true;
	}

	protected _isSelectedItem(unique: string | null): boolean {
		return this._treeContext?.selection.isSelected(unique) ?? false;
	}

	protected _selectItem(unique: string | null) {
		this._treeContext?.selection.select(unique);
	}

	protected _deselectItem(unique: string | null) {
		this._treeContext?.selection.deselect(unique);
	}
}
