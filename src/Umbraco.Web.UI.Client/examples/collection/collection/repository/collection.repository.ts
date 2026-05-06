import type { ExampleCollectionFilterModel, ExampleCollectionItemModel } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';

export class ExampleCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository<ExampleCollectionItemModel, ExampleCollectionFilterModel>
{
	async requestCollection(args: ExampleCollectionFilterModel) {
		const skip = args.skip || 0;
		const take = args.take || 10;

		// Simulating a data fetch. This would in most cases be replaced with an API call.
		let items = [
			{
				unique: '3e31e9c5-7d66-4c99-a9e5-d9f2b1e2b22f',
				entityType: 'example',
				name: 'Example Item 1',
				icon: 'icon-newspaper',
			},
			{
				unique: 'bc9b6e24-4b11-4dd6-8d4e-7c4f70e59f3c',
				entityType: 'example',
				name: 'Example Item 2',
				icon: 'icon-newspaper',
			},
			{
				unique: '5a2f4e3a-ef7e-470e-8c3c-3d859c02ae0d',
				entityType: 'example',
				name: 'Example Item 3',
				icon: 'icon-newspaper',
			},
			{
				unique: 'f4c3d8b8-6d79-4c87-9aa9-56b1d8fda702',
				entityType: 'example',
				name: 'Example Item 4',
				icon: 'icon-newspaper',
			},
			{
				unique: 'c9f0a8a3-1b49-4724-bde3-70e31592eb6e',
				entityType: 'example',
				name: 'Example Item 5',
				icon: 'icon-newspaper',
			},
		];

		// Simulating filtering based on the args
		if (args.filter) {
			items = items.filter((item) => item.name.toLowerCase().includes(args.filter!.toLowerCase()));
		}

		// Get the total number of items before pagination
		const totalItems = items.length;

		// Simulating pagination
		const start = skip;
		const end = start + take;
		items = items.slice(start, end);

		const data = {
			items,
			total: totalItems,
		};

		return { data };
	}
}

export { ExampleCollectionRepository as api };
