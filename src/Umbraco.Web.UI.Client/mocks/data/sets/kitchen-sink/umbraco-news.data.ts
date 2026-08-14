import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<NewsDashboardItemResponseModel> = [
	{
		priority: 'High',
		header: 'Welcome to the Kitchen Sink mock data set!',
		body: `
			<strong>Note:</strong> This is a preview version of the Umbraco Backoffice using the kitchen-sink data set.
		`,
		buttonText: 'Read more about Umbraco CMS',
		imageUrl: '',
		imageAltText: '',
		url: 'https://umbraco.com/products/umbraco-cms/',
	},
];
