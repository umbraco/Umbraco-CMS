import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbWithDescriptionModel } from '@umbraco-cms/backoffice/models';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type {
	UmbPickerCollectionDataSource,
	UmbPickerCollectionDataSourceTextFilterFeature,
} from '@umbraco-cms/backoffice/picker-data-source';

interface ExampleCollectionItemModel extends UmbCollectionItemModel, UmbWithDescriptionModel {
	isPickable: boolean;
}

export class ExampleCustomPickerCollectionPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<ExampleCollectionItemModel>
{
	#supportsTextFilter = new UmbObjectState<UmbPickerCollectionDataSourceTextFilterFeature>({ enabled: true });

	public readonly features = {
		supportsTextFilter: this.#supportsTextFilter.asObservable(),
	};

	collectionPickableFilter = (item: ExampleCollectionItemModel) => item.isPickable;

	async requestCollection(args: UmbCollectionFilterModel) {
		const skip = args.skip ?? 0;
		const take = args.take ?? 100;

		const filterText = args.filter?.toLowerCase() ?? '';

		const filteredItems = filterText
			? customItems.filter((item) => item.name?.toLowerCase().includes(filterText))
			: customItems;

		const paginatedItems = filteredItems.slice(skip, skip + take);

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
}

export { ExampleCustomPickerCollectionPropertyEditorDataSource as api };

const customItems: Array<ExampleCollectionItemModel> = [
	{
		unique: '1',
		entityType: 'example',
		name: 'Example 1',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 1',
		isPickable: true,
	},
	{
		unique: '2',
		entityType: 'example',
		name: 'Example 2',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 2',
		isPickable: true,
	},
	{
		unique: '3',
		entityType: 'example',
		name: 'Example 3',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 3',
		isPickable: true,
	},
	{
		unique: '4',
		entityType: 'example',
		name: 'Example 4',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 4',
		isPickable: false,
	},
	{
		unique: '5',
		entityType: 'example',
		name: 'Example 5',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 5',
		isPickable: true,
	},
	{
		unique: '6',
		entityType: 'example',
		name: 'Example 6',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 6',
		isPickable: true,
	},
	{
		unique: '7',
		entityType: 'example',
		name: 'Example 7',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 7',
		isPickable: true,
	},
	{
		unique: '8',
		entityType: 'example',
		name: 'Example 8',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 8',
		isPickable: true,
	},
	{
		unique: '9',
		entityType: 'example',
		name: 'Example 9',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 9',
		isPickable: false,
	},
	{
		unique: '10',
		entityType: 'example',
		name: 'Example 10',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 10',
		isPickable: true,
	},
	{
		unique: '11',
		entityType: 'example',
		name: 'Example 11',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 11',
		isPickable: true,
	},
	{
		unique: '12',
		entityType: 'example',
		name: 'Example 12',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 12',
		isPickable: true,
	},
	{
		unique: '13',
		entityType: 'example',
		name: 'Example 13',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 13',
		isPickable: true,
	},
	{
		unique: '14',
		entityType: 'example',
		name: 'Example 14',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 14',
		isPickable: false,
	},
	{
		unique: '15',
		entityType: 'example',
		name: 'Example 15',
		icon: 'icon-shape-triangle blue',
		description: 'This is an example item 15',
		isPickable: true,
	},
];
