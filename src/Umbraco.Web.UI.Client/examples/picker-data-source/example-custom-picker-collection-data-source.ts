import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type {
	UmbPickerCollectionDataSource,
	UmbPickerSearchableDataSource,
} from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

interface ExampleCollectionItemModel extends UmbCollectionItemModel {
	isPickable: boolean;
}

export class ExampleCustomPickerCollectionPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<ExampleCollectionItemModel>, UmbPickerSearchableDataSource
{
	collectionPickableFilter = (item: ExampleCollectionItemModel) => item.isPickable;

	async requestCollection(args: UmbCollectionFilterModel) {
		const skip = args.skip ?? 0;
		const take = args.take ?? 100;

		const paginatedItems = customItems.slice(skip, skip + take);

		const data = {
			items: paginatedItems,
			total: customItems.length,
		};

		return { data };
	}

	async requestItems(uniques: Array<string>) {
		const items = customItems.filter((x) => uniques.includes(x.unique));
		return { data: items };
	}

	async search(args: UmbSearchRequestArgs) {
		const skip = args.paging?.skip ?? 0;
		const take = args.paging?.take ?? 100;

		const filteredItems = customItems.filter((item) => item.name?.toLowerCase().includes(args.query.toLowerCase()));
		const paginatedItems = filteredItems.slice(skip, skip + take);

		const data = {
			items: paginatedItems,
			total: filteredItems.length,
		};

		return { data };
	}
}

export { ExampleCustomPickerCollectionPropertyEditorDataSource as api };

const customItems: Array<ExampleCollectionItemModel> = [
	{
		unique: '1',
		entityType: 'example',
		name: 'Example 1',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '2',
		entityType: 'example',
		name: 'Example 2',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '3',
		entityType: 'example',
		name: 'Example 3',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '4',
		entityType: 'example',
		name: 'Example 4',
		icon: 'icon-shape-triangle blue',
		isPickable: false,
	},
	{
		unique: '5',
		entityType: 'example',
		name: 'Example 5',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '6',
		entityType: 'example',
		name: 'Example 6',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '7',
		entityType: 'example',
		name: 'Example 7',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '8',
		entityType: 'example',
		name: 'Example 8',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '9',
		entityType: 'example',
		name: 'Example 9',
		icon: 'icon-shape-triangle blue',
		isPickable: false,
	},
	{
		unique: '10',
		entityType: 'example',
		name: 'Example 10',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '11',
		entityType: 'example',
		name: 'Example 11',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '12',
		entityType: 'example',
		name: 'Example 12',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '13',
		entityType: 'example',
		name: 'Example 13',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
	{
		unique: '14',
		entityType: 'example',
		name: 'Example 14',
		icon: 'icon-shape-triangle blue',
		isPickable: false,
	},
	{
		unique: '15',
		entityType: 'example',
		name: 'Example 15',
		icon: 'icon-shape-triangle blue',
		isPickable: true,
	},
];
