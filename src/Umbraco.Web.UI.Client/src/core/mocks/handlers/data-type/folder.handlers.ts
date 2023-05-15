import { rest } from 'msw';
import { umbDataTypeData } from '../../data/data-type.data';
import { slug } from './slug';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

export const folderHandlers = [
	rest.post(umbracoPath(`${slug}/folder`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.createFolder(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${slug}/folder/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dataType = umbDataTypeData.getById(id);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath(`${slug}/folder/:id`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.save(data);

		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath(`${slug}/folder/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		try {
			umbDataTypeData.deleteFolder(id);
			return res(ctx.status(200));
		} catch (error) {
			return res(
				ctx.status(404),
				ctx.json<ProblemDetailsModel>({
					status: 404,
					type: 'error',
					detail: 'Not Found',
				})
			);
		}
	}),
];
