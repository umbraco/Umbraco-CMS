import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<NewsDashboardItemResponseModel> = [
	{
		priority: 'High',
		header: 'Welcome to the Multi Brand Clothing Shop mock data set!',
		body: `
			<strong>Note:</strong> This is a preview version of the Umbraco Backoffice using the multi-brand-clothing-shop data set.
		`,
		buttonText: 'Read more about Umbraco CMS',
		imageUrl: '',
		imageAltText: '',
		url: 'https://umbraco.com/products/umbraco-cms/',
	},
];
