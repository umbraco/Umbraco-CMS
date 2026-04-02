import blueFlowersImgUrl from './images/blue-flowers-small.jpg';
import redflowersImgUrl from './images/red-flowers-small.jpg';
import branchImgUrl from './images/branch-small.jpg';
import grassImgUrl from './images/grass-small.jpg';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbCollectionFilterModel, UmbCollectionItemModel } from '@umbraco-cms/backoffice/collection';
import type { UmbWithThumbnailModel } from '@umbraco-cms/backoffice/models';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';

interface ExampleThumbnailCollectionItemModel extends UmbCollectionItemModel, UmbWithThumbnailModel {}

export class ExampleCustomPickerCollectionPropertyEditorDataSource
	extends UmbControllerBase
	implements UmbPickerCollectionDataSource<ExampleThumbnailCollectionItemModel>
{
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
}

export { ExampleCustomPickerCollectionPropertyEditorDataSource as api };

const customItems: Array<ExampleThumbnailCollectionItemModel> = [
	{
		unique: '1',
		entityType: 'example',
		name: 'Example 1',
		icon: 'icon-shape-triangle',
		thumbnail: {
			src: blueFlowersImgUrl,
			alt: 'Example 1 thumbnail',
		},
	},
	{
		unique: '2',
		entityType: 'example',
		name: 'Example 2',
		icon: 'icon-shape-triangle',
		thumbnail: {
			src: redflowersImgUrl,
			alt: 'Example 2 thumbnail',
		},
	},
	{
		unique: '3',
		entityType: 'example',
		name: 'Example 3',
		icon: 'icon-shape-triangle',
		thumbnail: null,
	},
	{
		unique: '4',
		entityType: 'example',
		name: 'Example 4',
		icon: 'icon-shape-triangle',
		thumbnail: {
			src: branchImgUrl,
			alt: 'Example 4 thumbnail',
		},
	},
	{
		unique: '5',
		entityType: 'example',
		name: 'Example 5',
		icon: 'icon-shape-triangle',
		thumbnail: {
			src: grassImgUrl,
			alt: 'Example 5 thumbnail',
		},
	},
];
