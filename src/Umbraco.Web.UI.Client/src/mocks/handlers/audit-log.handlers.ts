import { logs } from '../data/audit-log.data.js';
const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import {
	PagedAuditLogResponseModel,
	PagedAuditLogWithUsernameResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export const handlers = [
	rest.get(umbracoPath('/audit-log'), (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json<PagedAuditLogWithUsernameResponseModel>({ total: 0, items: [] }));
	}),
	rest.get(umbracoPath('/audit-log/:id'), (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json<PagedAuditLogResponseModel>({ total: 0, items: [] }));
	}),
	rest.get(umbracoPath('/audit-log/type/:logType'), (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json<PagedAuditLogResponseModel>({ total: 0, items: [] }));
	}),
];
