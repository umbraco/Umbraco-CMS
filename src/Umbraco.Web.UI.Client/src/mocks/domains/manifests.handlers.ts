import { rest } from 'msw';

import umbracoPath from '../../core/helpers/umbraco-path';

import type { ManifestsResponse } from '../../core/models';

export const manifestDevelopmentHandler = rest.get(umbracoPath('/manifests'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ManifestsResponse>({
			manifests: [
				{
					type: 'section',
					alias: 'My.Section.Custom',
					name: 'Custom Section',
					js: '/src/mocks/App_Plugins/section.js',
					elementName: 'my-section-custom',
					meta: {
						pathname: 'my-custom',
						weight: 1,
					},
				},
				{
					type: 'propertyEditorUI',
					alias: 'My.PropertyEditorUI.Custom',
					name: 'My Custom Property Editor UI',
					js: '/src/mocks/App_Plugins/property-editor.js',
					elementName: 'my-property-editor-ui-custom',
					meta: {
						icon: 'document',
						group: 'common',
					},
				},
				{
					type: 'entrypoint',
					alias: 'My.Entrypoint.Custom',
					js: '/src/mocks/App_Plugins/custom-entrypoint.js',
				},
			],
		})
	);
});

export const manifestEmptyHandler = rest.get(umbracoPath('/manifests'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ManifestsResponse>({
			manifests: [],
		})
	);
});
