const { http, HttpResponse } = window.MockServiceWorker;
import { umbDataTypeMockDb, type UmbDataTypeFilterOptions } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const filterHandlers = [
	http.get(umbracoPath(`/filter${UMB_SLUG}`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = Number(searchParams.get('skip'));
		const take = Number(searchParams.get('take'));
		const orderBy = searchParams.get('orderBy');
		const orderDirection = searchParams.get('orderDirection');
		const editorUiAlias = searchParams.get('editorUiAlias');
		const filter = searchParams.get('filter');

		const options: UmbDataTypeFilterOptions = {
			skip: skip || 0,
			take: take || 10,
			orderBy: orderBy || 'name',
			orderDirection: orderDirection || 'asc',
			editorUiAlias: editorUiAlias || undefined,
			filter: filter || undefined,
		};

		const response = umbDataTypeMockDb.filter(options);
		return HttpResponse.json(response);
	}),
];
