import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

// Temporary server type model until generate:server-api exists.
export type ServerNewsStory = {
	id: string | number;
	title: string;
	description?: string;
	imageUrl?: string | null;
	priority: 'High' | 'Medium' | 'Low';
	publishedAt: string;
	linkUrl?: string;
};

export interface UmbNewsStoriesDataSource {
	getAllNewsStories(): Promise<UmbDataSourceResponse<ServerNewsStory[]>>;
}
