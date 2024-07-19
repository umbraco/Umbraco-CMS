import { UmbEntityMockDbBase } from './entity-base.js';
import { UmbMockEntityTreeManager } from './entity-tree.manager.js';

export class UmbEntityRecycleBin<
	MockType extends { id: string; isTrashed: boolean; hasChildren: boolean },
> extends UmbEntityMockDbBase<MockType> {
	tree;

	constructor(data: Array<MockType>, treeItemMapper: (model: MockType) => any) {
		super(data);
		this.tree = new UmbMockEntityTreeManager<MockType>(this, treeItemMapper);
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
