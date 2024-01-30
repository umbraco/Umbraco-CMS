const { rest } = window.MockServiceWorker;
import { umbDictionaryMockDb } from '../../data/dictionary/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import type { ImportDictionaryRequestModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const uploadResponse: ImportDictionaryRequestModel = {
	temporaryFileId: 'c:/path/to/tempfilename.udt',
	parentId: 'b7e7d0ab-53ba-485d-dddd-12537f9925aa',
};

export const uploadHandlers = [
	rest.post(umbracoPath(`${UMB_SLUG}/upload`), async (req, res, ctx) => {
		debugger;
		if (!req.arrayBuffer()) return;

		return res(ctx.status(200), ctx.json(uploadResponse));
	}),
];
