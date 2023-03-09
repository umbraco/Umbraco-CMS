import { rest } from 'msw';
import { v4 as uuidv4 } from 'uuid';

import { umbracoPath } from '@umbraco-cms/utils';
import {
	PackageCreateModel,
	PackageDefinitionModel,
	PagedPackageDefinitionModel,
	PagedPackageMigrationStatusModel,
} from '@umbraco-cms/backend-api';

export const handlers = [
	rest.get(umbracoPath('/package/migration-status'), (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<PagedPackageMigrationStatusModel>({
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
			})
		);
	}),

	rest.post(umbracoPath('/package/:name/run-migration'), async (_req, res, ctx) => {
		const name = _req.params.name as string;
		if (!name) return res(ctx.status(404));
		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/package/created'), async (_req, res, ctx) => {
		// read all
		return res(
			ctx.status(200),
			ctx.json<PagedPackageDefinitionModel>({
				total: packageArray.length,
				items: packageArray,
			})
		);
	}),

	rest.post(umbracoPath('/package/created'), async (_req, res, ctx) => {
		//save
		const data: PackageCreateModel = await _req.json();
		const newPackage: PackageDefinitionModel = { ...data, key: uuidv4() };
		packageArray.push(newPackage);
		return res(ctx.status(200), ctx.json<PackageDefinitionModel>(newPackage));
	}),

	rest.get(umbracoPath('/package/created/:key'), (_req, res, ctx) => {
		//read 1
		const key = _req.params.key as string;
		if (!key) return res(ctx.status(404));
		const found = packageArray.find((p) => p.key == key);
		if (!found) return res(ctx.status(404));
		return res(ctx.status(200), ctx.json<PackageDefinitionModel>(found));
	}),

	rest.put(umbracoPath('/package/created/:key'), async (_req, res, ctx) => {
		//update
		const data: PackageDefinitionModel = await _req.json();
		if (!data.key) return;
		const index = packageArray.findIndex((x) => x.key === data.key);
		packageArray[index] = data;
		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/package/created/:key'), (_req, res, ctx) => {
		//delete
		const key = _req.params.key as string;
		if (!key) return res(ctx.status(404));
		const index = packageArray.findIndex((p) => p.key == key);
		if (index <= -1) return res(ctx.status(404));
		packageArray.splice(index, 1);
		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/package/created/:key/download'), (_req, res, ctx) => {
		//download
		return res(ctx.status(200));
	}),
];

const packageArray: PackageDefinitionModel[] = [
	{
		key: '2a0181ec-244b-4068-a1d7-2f95ed7e6da6',
		packagePath: undefined,
		name: 'My Package',
		//contentNodeId?: string | null;
		//contentLoadChildNodes?: boolean;
		//mediaKeys?: Array<string>;
		//mediaLoadChildNodes?: boolean;
		//documentTypes?: Array<string>;
		//mediaTypes?: Array<string>;
		//dataTypes?: Array<string>;
		//templates?: Array<string>;
		//partialViews?: Array<string>;
		//stylesheets?: Array<string>;
		//scripts?: Array<string>;
		//languages?: Array<string>;
		//dictionaryItems?: Array<string>;
	},
	{
		key: '2a0181ec-244b-4068-a1d7-2f95ed7e6da7',
		packagePath: undefined,
		name: 'My Second Package',
	},

	{
		key: '2a0181ec-244b-4068-a1d7-2f95ed7e6da8',
		packagePath: undefined,
		name: 'My Third Package',
	},
];
