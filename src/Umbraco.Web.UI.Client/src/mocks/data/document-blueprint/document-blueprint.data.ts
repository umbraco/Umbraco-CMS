import type { UmbMockDocumentModel } from '../document/document.data.js';
import { DocumentVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMockDocumentBlueprintModel extends UmbMockDocumentModel {}

export const data: Array<UmbMockDocumentBlueprintModel> = [
	{
		ancestors: [],
		urls: [
			{
				culture: 'en-US',
				url: '/',
			},
		],
		template: null,
		id: 'the-simplest-document-id',
		createDate: '2023-02-06T15:32:05.350038',
		parent: null,
		documentType: {
			id: 'the-simplest-document-type-id',
			icon: 'icon-document',
		},
		hasChildren: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: DocumentVariantStateModel.DRAFT,
				publishDate: '2023-02-06T15:32:24.957009',
				culture: 'en-us',
				segment: null,
				name: 'The Simplest Document Blueprint',
				createDate: '2023-02-06T15:32:05.350038',
				updateDate: '2023-02-06T15:32:24.957009',
			},
		],
		values: [
			{
				editorAlias: 'Umbraco.TextBox',
				alias: 'prop1',
				culture: null,
				segment: null,
				value: 'my blueprint value',
			},
		],
	},
];
