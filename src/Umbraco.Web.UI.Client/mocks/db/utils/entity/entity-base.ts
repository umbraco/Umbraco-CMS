import type { UmbMockDataSet } from '../../../data/mock-data-set.types.js';
import { UmbMockDBBase } from '../mock-db-base.js';

export abstract class UmbEntityMockDbBase<MockItemType extends { id: string }> extends UmbMockDBBase<MockItemType> {
	constructor(dataKey: keyof UmbMockDataSet, data: Array<MockItemType>) {
		super(dataKey, data);
	}

	create(item: MockItemType) {
		this.data.push(item);
	}

	read(id: string) {
		return this.data.find((item) => item.id === id);
	}

	update(id: string, updatedItem: MockItemType) {
		const itemIndex = this.data.findIndex((item) => item.id === id);
		this.data[itemIndex] = updatedItem;
	}

	/** Whether any entity sits directly under `id`. A trashed entity's children only count while they're trashed too. */
	hasChildren(id: string): boolean {
		const isTrashed = (this.read(id) as { isTrashed?: boolean } | undefined)?.isTrashed ?? false;
		return this.data.some((item) => {
			const candidate = item as { parent?: { id: string } | null; isTrashed?: boolean };
			return candidate.parent?.id === id && (candidate.isTrashed ?? false) === isTrashed;
		});
	}

	/** The ids of the entities above `id`, root first. */
	getAncestorIds(id: string): Array<{ id: string }> {
		const ancestors: Array<{ id: string }> = [];
		let parentId = (this.read(id) as { parent?: { id: string } | null } | undefined)?.parent?.id;
		while (parentId) {
			ancestors.unshift({ id: parentId });
			parentId = (this.read(parentId) as { parent?: { id: string } | null } | undefined)?.parent?.id;
		}
		return ancestors;
	}

	delete(id: string) {
		this.data = this.data.filter((item) => item.id !== id);
	}
}
