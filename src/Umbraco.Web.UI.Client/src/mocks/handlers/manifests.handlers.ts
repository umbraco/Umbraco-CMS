const { rest } = window.MockServiceWorker;

import type { PackageManifestResponse } from '../../packages/packages/types.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const manifestDevelopmentHandler = rest.get(umbracoPath('/package/manifest'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<PackageManifestResponse>([
			{
				name: 'Named Package',
				version: '1.0.0',
				extensions: [
					{
						type: 'section',
						alias: 'My.Section.Custom',
						name: 'Custom Section',
						js: '/App_Plugins/section.js',
						elementName: 'my-section-custom',
						weight: 1,
						meta: {
							label: 'Custom',
							pathname: 'my-custom',
						},
					},
					{
						type: 'propertyEditorUi',
						alias: 'My.PropertyEditorUI.Custom',
						name: 'My Custom Property Editor UI',
						js: '/App_Plugins/property-editor.js',
						elementName: 'my-property-editor-ui-custom',
						meta: {
							label: 'My Custom Property',
							icon: 'document',
							group: 'Common',
							propertyEditorSchema: 'Umbraco.TextBox',
						},
					},
				],
			},
			{
				extensions: [
					{
						type: 'entryPoint',
						name: 'My Custom Entry Point',
						alias: 'My.Entrypoint.Custom',
						js: '/App_Plugins/custom-entrypoint.js',
					},
				],
			},
			{
				name: 'Package with a view',
				extensions: [
					{
						type: 'packageView',
						alias: 'My.PackageView.Custom',
						name: 'My Custom Package View',
						js: '/App_Plugins/package-view.js',
						meta: {
							packageName: 'Package with a view',
						},
					},
				],
			},
		])
	);
});

export const manifestEmptyHandler = rest.get(umbracoPath('/package/manifest'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<PackageManifestResponse>([])
	);
});
