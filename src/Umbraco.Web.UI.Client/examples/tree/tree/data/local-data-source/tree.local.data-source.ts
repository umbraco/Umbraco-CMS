import type { ExampleTreeItemModel } from '../../types.js';
import { EXAMPLE_ENTITY_TYPE, EXAMPLE_ROOT_ENTITY_TYPE } from '../../../entity.js';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeDataSource,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

const EXAMPLE_TREE_DATA: Array<ExampleTreeItemModel> = [
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: false,
		isFolder: false,
		name: 'Item 1',
		parent: { unique: null, entityType: EXAMPLE_ROOT_ENTITY_TYPE },
		unique: 'ab7b6e03-5f4d-4a6b-9f4c-21292d462e08',
		icon: 'icon-newspaper',
	},
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: true,
		isFolder: false,
		name: 'Item 2',
		parent: { unique: null, entityType: EXAMPLE_ROOT_ENTITY_TYPE },
		unique: '74a5b2d9-3564-45b8-a3ee-98fc7ec0c1fb',
		icon: 'icon-newspaper',
	},
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: false,
		isFolder: false,
		name: 'Item 3',
		parent: { unique: null, entityType: EXAMPLE_ROOT_ENTITY_TYPE },
		unique: '1b8ed2ac-b4bb-4384-972e-2cf18f40586a',
		icon: 'icon-newspaper',
	},
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: false,
		isFolder: false,
		name: 'Item 2.1',
		parent: { unique: '74a5b2d9-3564-45b8-a3ee-98fc7ec0c1fb', entityType: EXAMPLE_ENTITY_TYPE },
		unique: '62dbd672-b198-4fc8-8b42-5d21dfbd3788',
		icon: 'icon-newspaper',
	},
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: true,
		isFolder: false,
		name: 'Item 2.2',
		parent: { unique: '74a5b2d9-3564-45b8-a3ee-98fc7ec0c1fb', entityType: EXAMPLE_ENTITY_TYPE },
		unique: 'deaa3f8c-e40b-4eb7-8268-34014504152e',
		icon: 'icon-newspaper',
	},
	{
		entityType: EXAMPLE_ENTITY_TYPE,
		hasChildren: false,
		isFolder: false,
		name: 'Item 2.2.1',
		parent: { unique: 'deaa3f8c-e40b-4eb7-8268-34014504152e', entityType: EXAMPLE_ENTITY_TYPE },
		unique: 'd4cf5fd2-1f84-4f3b-b63b-5f29a38e72d1',
		icon: 'icon-newspaper',
	},
];

export class ExampleTreeLocalDataSource extends UmbControllerBase implements UmbTreeDataSource<ExampleTreeItemModel> {
	async getRootItems(args: UmbTreeRootItemsRequestArgs) {
		// TODO: handle skip, take, foldersOnly.
		console.log(args);
		const rootItems: Array<ExampleTreeItemModel> = EXAMPLE_TREE_DATA.filter((item) => item.parent.unique === null);
		return { data: { items: rootItems, total: rootItems.length } };
	}

	async getChildrenOf(args: UmbTreeChildrenOfRequestArgs) {
		// TODO: handle skip, take, foldersOnly.
		const children = EXAMPLE_TREE_DATA.filter(
			(item) => item.parent.unique === args.parent.unique && item.parent.entityType === args.parent.entityType,
		);

		return { data: { items: children, total: children.length } };
	}

	async getAncestorsOf(args: UmbTreeAncestorsOfRequestArgs) {
		const ancestors = findAncestors(args.treeItem.unique, args.treeItem.entityType);
		return { data: ancestors };
	}
}

// Helper function to find ancestors recursively
const findAncestors = (unique: string, entityType: string): Array<ExampleTreeItemModel> => {
	const item = EXAMPLE_TREE_DATA.find((i) => i.unique === unique && i.entityType === entityType);

	if (!item || !item.parent || item.parent.unique === null) {
		return [];
	}

	const parent = EXAMPLE_TREE_DATA.find(
		(i) => i.unique === item.parent.unique && i.entityType === item.parent.entityType,
	);

	if (!parent) {
		return [];
	}

	return [parent, ...findAncestors(parent.unique, parent.entityType)];
};
