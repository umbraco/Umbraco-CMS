import { UmbMockEntityTreeManager } from './entity-tree.manager.js';

/**
 * Recycle bin helper class for document/media DBs.
 * This is NOT a standalone DB - it shares data with its parent DB.
 * It does not auto-register with the mock DB registry.
 */
export class UmbEntityRecycleBin<MockType extends { id: string; isTrashed: boolean; hasChildren: boolean }> {
	protected data: Array<MockType> = [];
	tree;

	constructor(data: Array<MockType>, treeItemMapper: (model: MockType) => any) {
		this.data = data;
		this.tree = new UmbMockEntityTreeManager<MockType>(this, treeItemMapper);
	}

	/**
	 * Update data reference (called by parent DB's setData).
	 * @param data
	 */
	setData(data: Array<MockType>) {
		this.data = data;
	}

	clear() {
		this.data = [];
	}

	getAll() {
		return this.data;
	}

	read(id: string) {
		return this.data.find((item) => item.id === id);
	}

	update(id: string, updatedItem: MockType) {
		const itemIndex = this.data.findIndex((item) => item.id === id);
		this.data[itemIndex] = updatedItem;
	}

	trash(ids: string[]) {
		const models = ids.map((id) => this.read(id)).filter((model) => !!model) as Array<MockType>;

		models.forEach((model) => {
			model.isTrashed = true;
		});

		models.forEach((model) => {
			this.update(model.id, model);
		});
	}
}
