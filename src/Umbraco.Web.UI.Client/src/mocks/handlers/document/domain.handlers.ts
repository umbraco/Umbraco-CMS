const { rest } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: temp handlers until we have a real API
export const domainHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/:id/domains`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const response = umbDocumentMockDb.getDomainsForDocument();
		return res(ctx.status(200), ctx.json(response));
	}),
];
