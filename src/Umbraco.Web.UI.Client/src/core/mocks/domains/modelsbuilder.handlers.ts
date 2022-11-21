import { rest } from 'msw';

import { umbracoPath } from '@umbraco-cms/utils';
import { CreatedResult, ModelsBuilder, OutOfDateStatus } from '@umbraco-cms/backend-api';

export const handlers = [
	rest.post(umbracoPath('/models-builder/build'), async (_req, res, ctx) => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<CreatedResult>({})
		);
	}),

	rest.get(umbracoPath('/models-builder/dashboard'), async (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json<ModelsBuilder>({
				mode: undefined,
				canGenerate: true,
				outOfDateModels: true,
				lastError: `[plugin:vite:import-analysis] Missing "./directives/unsafe-htl.js" export in "lit" package
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
at async TransformContext.transform (file:///C:/Users/Umbraco/Documents/Umbraco.CMS.Backoffice/node_modules/vit`,
				version: '13.0.0',
				modelsNamespace:
					"<p>ModelsBuilder is enabled, with the following configuration:</p><ul><li>The <strong>models mode</strong> is 'InMemoryAuto'. Strongly typed models are re-generated on startup and anytime schema changes (i.e. Content Type) are made. No recompilation necessary but the generated models are not available to code outside of Razor.</li><li>Models namespace is Umbraco.Cms.Web.Common.PublishedModels.</li><li>Tracking of <strong>out-of-date models</strong> is not enabled.</li></ul>",
				trackingOutOfDateModels: true,
			})
		);
	}),

	rest.get(umbracoPath('/models-builder/status'), async (_req, res, ctx) => {
		return res(
			// Respond with a 200 status code
			ctx.status(200),
			ctx.json<OutOfDateStatus>({})
		);
	}),
];
