const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedTagResponseModel, TagResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	rest.get(umbracoPath('/tag'), (_req, res, ctx) => {
		// didnt add culture logic here

		const query = _req.url.searchParams.get('query');
		if (!query || !query.length) return;

		const tagGroup = _req.url.searchParams.get('tagGroup') ?? 'default';
		const skip = parseInt(_req.url.searchParams.get('skip') ?? '0', 10);
		const take = parseInt(_req.url.searchParams.get('take') ?? '5', 10);

		const TagsByGroup = TagData.filter((tag) => tag.group?.toLocaleLowerCase() === tagGroup.toLocaleLowerCase());
		const TagsMatch = TagsByGroup.filter((tag) => tag.text?.toLocaleLowerCase().includes(query.toLocaleLowerCase()));

		const Tags = TagsMatch.slice(skip, skip + take);

		const PagedData: PagedTagResponseModel = {
			total: Tags.length,
			items: Tags,
		};

		return res(ctx.status(200), ctx.json<PagedTagResponseModel>(PagedData));
	}),
];

// Mock Data

const TagData: TagResponseModel[] = [
	{
		id: '1',
		text: 'Cranberry',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '2',
		text: 'Kiwi',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '3',
		text: 'Blueberries',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '4',
		text: 'Watermelon',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '5',
		text: 'Tomato',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '6',
		text: 'Mango',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '7',
		text: 'Strawberry',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '8',
		text: 'Water Chestnut',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '9',
		text: 'Papaya',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '10',
		text: 'Orange Rind',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '11',
		text: 'Olives',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '12',
		text: 'Pear',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '13',
		text: 'Sultana',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '14',
		text: 'Mulberry',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '15',
		text: 'Lychee',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '16',
		text: 'Lemon',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '17',
		text: 'Apple',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '18',
		text: 'Banana',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '19',
		text: 'Dragonfruit',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '20',
		text: 'Blackberry',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '21',
		text: 'Raspberry',
		group: 'Fruits',
		nodeCount: 1,
	},
	{
		id: '22',
		text: 'Flour',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
	{
		id: '23',
		text: 'Eggs',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
	{
		id: '24',
		text: 'Butter',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
	{
		id: '25',
		text: 'Sugar',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
	{
		id: '26',
		text: 'Salt',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
	{
		id: '26',
		text: 'Milk',
		group: 'Cake Ingredients',
		nodeCount: 1,
	},
];
