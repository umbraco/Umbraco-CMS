import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNewsStoriesDataSource, ServerNewsStory } from './index.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

const MOCKDATA: ServerNewsStory[] = [
	{
		id: '1',
		title: 'Umbraco 13.2 released',
		description: 'Perf + backoffice tweaks.',
		imageUrl: 'https://picsum.photos/seed/umbraco132/600/320',
		priority: 'High',
		publishedAt: '2025-09-30',
		linkUrl: 'https://umbraco.com/blog/umbraco-13-2',
	},
	{
		id: '2',
		title: 'Package of the week',
		description: 'New community package!',
		imageUrl: 'https://picsum.photos/seed/pow/600/320',
		priority: 'Medium',
		publishedAt: '2025-09-28',
		linkUrl: 'https://our.umbraco.com/',
	},
	{
		id: '3',
		title: 'Community spotlight',
		description: 'Highlights from the community.',
		imageUrl: null,
		priority: 'Low',
		publishedAt: '2025-09-25',
		linkUrl: 'https://umbraco.com/community/',
	},
];

/**
 * A data source for the news stories
 * @class UmbNewsStoriesMockDataSource
 * @implements {UmbNewsStoriesDataSource}
 */

export class UmbNewsStoriesMockDataSource implements UmbNewsStoriesDataSource {
	#host: UmbControllerHost;
	/**
	 * Creates an instance of UmbNewsStoriesMockDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbNewsStoriesMockDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Grabs all the news stories from the server
	 * @returns {*}
	 * @memberof UmbNewsStoriesMockDataSource
	 */
	async getAllNewsStories(): Promise<UmbDataSourceResponse<ServerNewsStory[]>> {
		return { data: MOCKDATA };
	}
}
