const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	PagedRedirectUrlResponseModel,
	RedirectUrlResponseModel,
	RedirectUrlStatusResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { RedirectStatusModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	rest.get(umbracoPath('/redirect-management'), (_req, res, ctx) => {
		const filter = _req.url.searchParams.get('filter');
		const skip = parseInt(_req.url.searchParams.get('skip') ?? '0', 10);
		const take = parseInt(_req.url.searchParams.get('take') ?? '20', 10);

		if (filter) {
			const filtered: RedirectUrlResponseModel[] = [];

			PagedRedirectUrlData.items.forEach((item) => {
				if (item.originalUrl?.includes(filter)) filtered.push(item);
			});
			const filteredPagedData: PagedRedirectUrlResponseModel = {
				total: filtered.length,
				items: filtered.slice(skip, skip + take),
			};
			return res(ctx.status(200), ctx.json<PagedRedirectUrlResponseModel>(filteredPagedData));
		} else {
			const items = PagedRedirectUrlData.items.slice(skip, skip + take);

			const PagedData: PagedRedirectUrlResponseModel = {
				total: PagedRedirectUrlData.total,
				items,
			};
			return res(ctx.status(200), ctx.json<PagedRedirectUrlResponseModel>(PagedData));
		}
	}),

	rest.get(umbracoPath('/redirect-management/:id'), async (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return res(ctx.status(404));
		if (id === 'status') return res(ctx.status(200), ctx.json<RedirectUrlStatusResponseModel>(UrlTracker));

		const PagedRedirectUrlObject = _getRedirectUrlByKey(id);

		return res(ctx.status(200), ctx.json<PagedRedirectUrlResponseModel>(PagedRedirectUrlObject));
	}),

	rest.delete(umbracoPath('/redirect-management/:id'), async (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return res(ctx.status(404));

		const PagedRedirectUrlObject = _deleteRedirectUrlByKey(id);

		return res(ctx.status(200), ctx.json<any>(PagedRedirectUrlObject));
	}),

	/*rest.get(umbracoPath('/redirect-management/status'), (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json<RedirectUrlStatus>(UrlTracker));
	}),*/

	rest.post(umbracoPath('/redirect-management/status'), async (_req, res, ctx) => {
		UrlTracker.status =
			UrlTracker.status === RedirectStatusModel.ENABLED ? RedirectStatusModel.DISABLED : RedirectStatusModel.ENABLED;
		return res(ctx.status(200), ctx.json<any>(UrlTracker.status));
	}),
];

// Mock Data

const UrlTracker: RedirectUrlStatusResponseModel = { status: RedirectStatusModel.ENABLED, userIsAdmin: true };

const _getRedirectUrlByKey = (id: string) => {
	const PagedResult: PagedRedirectUrlResponseModel = {
		total: 0,
		items: [],
	};
	RedirectUrlData.forEach((data) => {
		if (data.id?.includes(id)) {
			PagedResult.items.push(data);
			PagedResult.total++;
		}
	});
	return PagedResult;
};

const _deleteRedirectUrlByKey = (id: string) => {
	const index = RedirectUrlData.findIndex((data) => data.id === id);
	if (index > -1) RedirectUrlData.splice(index, 1);
	const PagedResult: PagedRedirectUrlResponseModel = {
		items: RedirectUrlData,
		total: RedirectUrlData.length,
	};
	return PagedResult;
};

const RedirectUrlData: RedirectUrlResponseModel[] = [
	{
		id: '1',
		created: '2022-12-05T13:59:43.6827244',
		destinationUrl: 'kitty.com',
		originalUrl: 'kitty.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9f2' },
	},
	{
		id: '2',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'umbraco.com',
		originalUrl: 'umbraco.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9f' },
	},
	{
		id: '3',
		created: '2022-12-05T13:59:43.6827244',
		destinationUrl: 'uui.umbraco.com',
		originalUrl: 'uui.umbraco.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9f23' },
	},
	{
		id: '4',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'umbracoffee.com',
		originalUrl: 'umbracoffee.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9fdsaa' },
	},
	{
		id: '5',
		created: '2022-12-05T13:59:43.6827244',
		destinationUrl: 'section/settings',
		originalUrl: 'section/settings/123',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9f2e23' },
	},
	{
		id: '6',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'dxp.com',
		originalUrl: 'dxp.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9fsafsfd' },
	},
	{
		id: '7',
		created: '2022-12-05T13:59:43.6827244',
		destinationUrl: 'google.com',
		originalUrl: 'google.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9f2cxza' },
	},
	{
		id: '8',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'unicorns.com',
		originalUrl: 'unicorns.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31d9fweds' },
	},
	{
		id: '9',
		created: '2022-12-05T13:59:43.6827244',
		destinationUrl: 'h5yr.com',
		originalUrl: 'h5yr.dk',
		document: { id: '7191c911-6747-4824-849e-5208e2b31ddsfsdsfadsfdx9f2' },
	},
	{
		id: '10',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'our.umbraco.com',
		originalUrl: 'our.umbraco.dk',
		document: { id: '7191c911-6747-4824-849e-52dsacx08e2b31d9dsafdsff' },
	},
	{
		id: '11',
		created: '2022-13-05T13:59:43.6827244',
		destinationUrl: 'your.umbraco.com',
		originalUrl: 'your.umbraco.dk',
		document: { id: '7191c911-6747-4824-849e-52dsacx08e2b31d9fsda' },
	},
];

const PagedRedirectUrlData: PagedRedirectUrlResponseModel = {
	total: RedirectUrlData.length,
	items: RedirectUrlData,
};
