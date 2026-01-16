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
		// TODO: use args to filter/paginate etc
		console.log(args);
		const data = {
			items: customItems,
			total: customItems.length,
		};

		return { data };
	}

	async requestItems(uniques: Array<string>) {
		const items = customItems.filter((x) => uniques.includes(x.unique));
		return { data: items };
	}

	async search(args: UmbSearchRequestArgs) {
		const items = customItems.filter((item) => item.name?.toLowerCase().includes(args.query.toLowerCase()));
		const total = items.length;

		const data = {
			items,
			total,
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
];
