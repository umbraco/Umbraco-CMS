const { http, HttpResponse } = window.MockServiceWorker;
import { UmbId } from '@umbraco-cms/backoffice/id';

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	PagedPackageMigrationStatusResponseModel,
	CreatePackageRequestModel,
	GetPackageConfigurationResponse,
	PagedPackageDefinitionResponseModel,
	PackageDefinitionResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/package/configuration'), () => {
		return HttpResponse.json<GetPackageConfigurationResponse>({
			marketplaceUrl: 'https://marketplace.umbraco.com',
		});
	}),

	http.get(umbracoPath('/package/migration-status'), () => {
		return HttpResponse.json<PagedPackageMigrationStatusResponseModel>({
			total: 3,
			items: [
				{
					hasPendingMigrations: true,
					packageName: 'Named Package',
				},
				{
					hasPendingMigrations: true,
					packageName: 'My Custom Migration',
				},
				{
					hasPendingMigrations: false,
					packageName: 'Package with a view',
				},
			],
		});
	}),

	http.post<{ name: string }>(umbracoPath('/package/:name/run-migration'), async ({ params }) => {
		const name = params.name;
		if (!name) return new HttpResponse(null, { status: 404 });
		return new HttpResponse(null, { status: 200 });
	}),

	http.get(umbracoPath('/package/created'), async () => {
		// read all
		return HttpResponse.json<PagedPackageDefinitionResponseModel>({
			total: packageArray.length,
			items: packageArray,
		});
	}),

	http.post<object, CreatePackageRequestModel>(umbracoPath('/package/created'), async ({ request }) => {
		//save
		const data = await request.json();
		const newPackage: PackageDefinitionResponseModel = { ...data, id: UmbId.new(), packagePath: '' };
		packageArray.push(newPackage);
		return HttpResponse.json<PackageDefinitionResponseModel>(newPackage);
	}),

	http.get<{ id: string }>(umbracoPath('/package/created/:id'), ({ params }) => {
		//read 1
		const id = params.id;
		if (!id) return new HttpResponse(null, { status: 404 });
		const found = packageArray.find((p) => p.id == id);
		if (!found) return new HttpResponse(null, { status: 404 });
		return HttpResponse.json<PackageDefinitionResponseModel>(found);
	}),

	http.put<{ id: string }, PackageDefinitionResponseModel>(umbracoPath('/package/created/:id'), async ({ request }) => {
		//update
		const data = await request.json();
		if (!data.id) return;
		const index = packageArray.findIndex((x) => x.id === data.id);
		packageArray[index] = data;
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete<{ id: string }>(umbracoPath('/package/created/:id'), ({ params }) => {
		//delete
		const id = params.id;
		if (!id) return new HttpResponse(null, { status: 404 });
		const index = packageArray.findIndex((p) => p.id == id);
		if (index <= -1) return new HttpResponse(null, { status: 404 });
		packageArray.splice(index, 1);
		return new HttpResponse(null, { status: 200 });
	}),

	http.get(umbracoPath('/package/created/:id/download'), () => {
		//download
		return new HttpResponse(null, { status: 200 });
	}),
];

const packageArray: PackageDefinitionResponseModel[] = [
	{
		id: '2a0181ec-244b-4068-a1d7-2f95ed7e6da6',
		packagePath: '',
		name: 'My Package',
		contentNodeId: null,
		contentLoadChildNodes: false,
		mediaIds: [],
		mediaLoadChildNodes: false,
		documentTypes: [],
		mediaTypes: [],
		dataTypes: [],
		templates: [],
		partialViews: [],
		stylesheets: [],
		scripts: [],
		languages: [],
		dictionaryItems: [],
	},
	{
		id: '2a0181ec-244b-4068-a1d7-2f95ed7e6da7',
		packagePath: '',
		name: 'My Second Package',
		contentNodeId: null,
		contentLoadChildNodes: false,
		mediaIds: [],
		mediaLoadChildNodes: false,
		documentTypes: [],
		mediaTypes: [],
		dataTypes: [],
		templates: [],
		partialViews: [],
		stylesheets: [],
		scripts: [],
		languages: [],
		dictionaryItems: [],
	},

	{
		id: '2a0181ec-244b-4068-a1d7-2f95ed7e6da8',
		packagePath: '',
		name: 'My Third Package',
		contentNodeId: null,
		contentLoadChildNodes: false,
		mediaIds: [],
		mediaLoadChildNodes: false,
		documentTypes: [],
		mediaTypes: [],
		dataTypes: [],
		templates: [],
		partialViews: [],
		stylesheets: [],
		scripts: [],
		languages: [],
		dictionaryItems: [],
	},
];
