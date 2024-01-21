import { UmbEntityMockDbBase } from './entity-base.js';
import { UmbMockEntityTreeManager } from './entity-tree.manager.js';
import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbEntityRecycleBin<
	MockType extends Omit<EntityTreeItemResponseModel, 'type'>,
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
