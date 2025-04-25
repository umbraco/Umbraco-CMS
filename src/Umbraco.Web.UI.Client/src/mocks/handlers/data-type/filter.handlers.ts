const { rest } = window.MockServiceWorker;
import { umbDataTypeMockDb, type DataTypeFilterOptions } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const filterHandlers = [
	rest.get(umbracoPath(`/filter${UMB_SLUG}`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const orderBy = req.url.searchParams.get('orderBy');
		const orderDirection = req.url.searchParams.get('orderDirection');
		const editorUiAlias = req.url.searchParams.get('editorUiAlias');
		const filter = req.url.searchParams.get('filter');

		const options: DataTypeFilterOptions = {
			skip: skip || 0,
			take: take || 10,
			orderBy: orderBy || 'name',
			orderDirection: orderDirection || 'asc',
			editorUiAlias: editorUiAlias || undefined,
			filter: filter || undefined,
		};

		const response = umbDataTypeMockDb.filter(options);
		return res(ctx.status(200), ctx.json(response));
	}),
];
