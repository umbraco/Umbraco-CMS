import type { NewsDashboardItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const data: Array<NewsDashboardItemResponseModel> = [
	{
		priority: 'High',
		header: 'Welcome to the Kenn Mock Data Set!',
		body: `
			<p>This mock data set was generated from a real Umbraco SQLite database export. It contains actual document types, media types, content, and more.</p>
			<strong>Note:</strong> This is a preview version of the Umbraco Backoffice using the kenn data set.
		`,
		buttonText: 'Read more about Umbraco CMS',
		imageUrl: '',
		imageAltText: '',
		url: 'https://umbraco.com/products/umbraco-cms/',
	},
];
