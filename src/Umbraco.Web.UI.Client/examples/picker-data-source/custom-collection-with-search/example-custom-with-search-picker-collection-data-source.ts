import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbSearchRequestArgs, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

/**
 * A collection data source that supports search but deliberately not text filtering.
 */
export class ExampleCustomWithSearchPickerCollectionPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<UmbCollectionItemModel>
{
	async requestCollection(args: UmbCollectionFilterModel) {
		const skip = args.skip ?? 0;
		const take = args.take ?? 100;

		// No text filter support, so `args.filter` is deliberately ignored here.
		const data = {
			items: customItems.slice(skip, skip + take),
			total: customItems.length,
		};

		return { data };
	}

	async requestItems(uniques: Array<string>) {
		const items = customItems.filter((item) => uniques.includes(item.unique));
		return { data: items };
	}

	async search(args: UmbSearchRequestArgs) {
		const query = args.query.toLowerCase();
		const matches: Array<UmbSearchResultItemModel> = customItems.filter((item) =>
			item.name?.toLowerCase().includes(query),
		);

		const skip = args.paging?.skip ?? 0;
		const take = args.paging?.take ?? matches.length;

		const data = {
			items: matches.slice(skip, skip + take),
			total: matches.length,
		};

		return { data };
	}
}

export { ExampleCustomWithSearchPickerCollectionPropertyEditorDataSource as api };

const customItems: Array<UmbCollectionItemModel> = [
	{
		unique: '1',
		entityType: 'example',
		name: 'Example 1',
		icon: 'icon-shape-triangle',
	},
	{
		unique: '2',
		entityType: 'example',
		name: 'Example 2',
		icon: 'icon-shape-triangle',
	},
	{
		unique: '3',
		entityType: 'example',
		name: 'Example 3',
		icon: 'icon-shape-triangle',
	},
	{
		unique: '4',
		entityType: 'example',
		name: 'Example 4',
		icon: 'icon-shape-triangle',
	},
	{
		unique: '5',
		entityType: 'example',
		name: 'Example 5',
		icon: 'icon-shape-triangle',
	},
];
