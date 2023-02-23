import { rest } from 'msw';

import { umbracoPath } from '@umbraco-cms/utils';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

// TODO: Update to server API types when they are available & rename endpoint when we know what it is called
type Package = {
	name?: string;
	version?: string;
	extensions?: ManifestTypes[];
};
type ManifestsResponse = { items: Package[] };

export const manifestDevelopmentHandler = rest.get(umbracoPath('/manifests'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ManifestsResponse>({
			items: [
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
							type: 'propertyEditorUI',
							alias: 'My.PropertyEditorUI.Custom',
							name: 'My Custom Property Editor UI',
							js: '/App_Plugins/property-editor.js',
							elementName: 'my-property-editor-ui-custom',
							meta: {
								label: 'My Custom Property',
								icon: 'document',
								group: 'Common',
								propertyEditorModel: 'Umbraco.JSON',
							},
						},
					],
				},
				{
					extensions: [
						{
							type: 'entrypoint',
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
								packageAlias: 'my.package',
							},
						},
					],
				},
			],
		})
	);
});

export const manifestEmptyHandler = rest.get(umbracoPath('/manifests'), (_req, res, ctx) => {
	return res(
		// Respond with a 200 status code
		ctx.status(200),
		ctx.json<ManifestsResponse>({ items: [] })
	);
});
