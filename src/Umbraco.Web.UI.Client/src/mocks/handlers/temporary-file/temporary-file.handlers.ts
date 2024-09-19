import { rest } from 'msw';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PostTemporaryFileResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbId } from '@umbraco-cms/backoffice/id';

const UMB_SLUG = 'temporary-file';

export const handlers = [
	rest.post(umbracoPath(`/${UMB_SLUG}`), async (_req, res, ctx) => {
		return res(ctx.delay(), ctx.status(201), ctx.text<PostTemporaryFileResponse>(UmbId.new()));
	}),
];
