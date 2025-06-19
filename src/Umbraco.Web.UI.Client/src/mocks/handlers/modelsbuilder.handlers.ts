const { rest } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type {
	ModelsBuilderResponseModel,
	OutOfDateStatusResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { ModelsModeModel, OutOfDateTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	rest.post(umbracoPath('/models-builder/build'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		model = modelAfterBuild;
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json({}),
		);
	}),

	rest.get(umbracoPath('/models-builder/dashboard'), async (_req, res, ctx) => {
		return res(ctx.status(200), ctx.json<ModelsBuilderResponseModel>(model));
	}),

	rest.get(umbracoPath('/models-builder/status'), async (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<OutOfDateStatusResponseModel>({
				status: OutOfDateTypeModel.CURRENT,
			}),
		);
	}),
];

// Mock Data for now
const modelBeforeBuild: ModelsBuilderResponseModel = {
	mode: ModelsModeModel.IN_MEMORY_AUTO,
	canGenerate: true,
	outOfDateModels: true,
	lastError: `[plugin:vite:import-analysis] Missing "./directives/unsafe-html.js" export in "lit" package
C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/src/backoffice/dashboards/models-builder/dashboard-models-builder.element.ts
at bail (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:32675:8)
at resolve (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:32752:10)
at resolveExports (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:34128:12)
at resolveDeepImport (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:34146:31)
at tryNodeResolve (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:33838:20)
at Context.resolveId (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:33598:28)
at async Object.resolveId (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:40156:32)
at async TransformContext.resolve (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:39921:23)
at async normalizeUrl (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite/dist/node/chunks/dep-67e7f8ab.js:36831:34)
at async TransformContext.transform (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vite`,
	version: '13.0.0',
	modelsNamespace: 'Umbraco.Cms.Web.Common.PublishedModels',
	trackingOutOfDateModels: true,
};

const modelAfterBuild: ModelsBuilderResponseModel = {
	mode: ModelsModeModel.IN_MEMORY_AUTO,
	canGenerate: true,
	outOfDateModels: false,
	lastError: '',
	version: '13.0.0',
	modelsNamespace: 'Umbraco.Cms.Web.Common.PublishedModels',
	trackingOutOfDateModels: true,
};

let model = modelBeforeBuild;
