const { rest } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const allowedTypesHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/allowed-document-types`), (req, res, ctx) => {
		debugger;
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbDocumentMockDb.getAllowedDocumentTypes(id);
		return res(ctx.status(200), ctx.json(response));
	}),
];
