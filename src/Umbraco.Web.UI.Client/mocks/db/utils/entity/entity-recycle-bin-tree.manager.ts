import { UmbMockEntityTreeManager } from './entity-tree.manager.js';

/** Tree manager for the recycle bin: shows only trashed items — the inverse of the base class's default. */
export class UmbMockEntityRecycleBinTreeManager<
	T extends { id: string; parent?: { id: string } | null; hasChildren: boolean; isTrashed: boolean },
> extends UmbMockEntityTreeManager<T> {
	protected override _isItemVisible(item: T): boolean {
		return item.isTrashed === true;
	}
}
