import { UmbEntityMockDbBase } from './entity-base.js';
import { UmbMockEntityTreeManager } from './entity-tree.manager.js';

export class UmbEntityRecycleBin<
	MockType extends { id: string; parent?: { id: string } | null; isTrashed: boolean; hasChildren: boolean },
> extends UmbEntityMockDbBase<MockType> {
	tree;

	constructor(data: Array<MockType>, treeItemMapper: (model: MockType) => any) {
		super(data);
		this.tree = new UmbMockEntityTreeManager<MockType>(this, treeItemMapper);
	}

	emptyRecycleBin() {
		const trashedItems = this.getAll().filter((item) => item.isTrashed);
		trashedItems.forEach((item) => this.delete(item.id));
	}

	restore(id: string, parentId: string | null) {
		const model = this.read(id);
		if (!model) throw new Error(`Element with id ${id} not found`);

		model.isTrashed = false;
		model.parent = parentId ? { id: parentId } : null;

		this.update(id, model);
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
