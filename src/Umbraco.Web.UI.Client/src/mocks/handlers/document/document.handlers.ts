const { rest } = window.MockServiceWorker;
import { umbDocumentData } from '../../data/document.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/document/root/allowed-document-types'), (req, res, ctx) => {
		const response = umbDocumentData.getAllowedDocumentTypesAtRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath('/document/:id/move-to-recycle-bin'), async (req, res, ctx) => {
		const id = req.params.id as string;
		umbDocumentData.trash([id]);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath('/document/:id/publish'), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;
		umbDocumentData.publish(id, data);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath('/document/:id/unpublish'), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;
		umbDocumentData.unpublish(id, data);
		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/document/:id/allowed-document-types'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const response = umbDocumentData.getDocumentByIdAllowedDocumentTypes(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/document/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const document = umbDocumentData.getById(id);
		return res(ctx.status(200), ctx.json(document));
	}),

	rest.delete(umbracoPath('/document/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		umbDocumentData.delete([id]);
		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`/document/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;
		umbDocumentData.save(id, data);
		return res(ctx.status(200));
	}),

	rest.post(umbracoPath(`/document`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		umbDocumentData.insert(data);
		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/document/:id/domains'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const response = umbDocumentData.getDomains(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put(umbracoPath('/document/:id/domains'), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;
		umbDocumentData.setDomains(id, data);
		return res(ctx.status(200));
	}),
];
