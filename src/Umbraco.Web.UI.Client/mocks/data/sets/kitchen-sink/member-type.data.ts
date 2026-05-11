import type { UmbMockMemberTypeModel } from '../../mock-data-set.types.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string composition type to enum
/**
 *
 * @param type
 */
function mapCompositionType(type: string): CompositionTypeModel {
	switch (type) {
		case 'Composition':
			return CompositionTypeModel.COMPOSITION;
		case 'Inheritance':
			return CompositionTypeModel.INHERITANCE;
		default:
			return CompositionTypeModel.COMPOSITION;
	}
}

const rawData: Array<
	Omit<UmbMockMemberTypeModel, 'compositions'> & {
		compositions: Array<{ memberType: { id: string }; compositionType: string }>;
	}
> = [
	{
		name: 'Member',
		id: 'd59be02f-1df9-4228-aa1e-01917d806cda',
		description: null,
		alias: 'Member',
		icon: 'icon-user',
		allowedAsRoot: false,
		variesByCulture: false,
		variesBySegment: false,
		isElement: false,
		properties: [
			{
				id: 'pt-28',
				container: {
					id: '0756729d-d665-46e3-b84a-37aceaa614f8',
				},
				alias: 'umbracoMemberComments',
				name: 'Comments',
				description: null,
				dataType: {
					id: 'c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3',
				},
				variesByCulture: false,
				variesBySegment: false,
				sortOrder: 0,
				validation: {
					mandatory: false,
					mandatoryMessage: null,
					regEx: null,
					regExMessage: null,
				},
				appearance: {
					labelOnTop: false,
				},
				isSensitive: false,
				visibility: {
					memberCanView: false,
					memberCanEdit: false,
				},
			},
		],
		containers: [
			{
				id: '0756729d-d665-46e3-b84a-37aceaa614f8',
				parent: null,
				name: 'Membership',
				type: 'Group',
				sortOrder: 1,
			},
		],
		compositions: [],
		parent: null,
		hasChildren: false,
		hasListView: false,
		isFolder: false,
		flags: [],
	},
];

export const data: Array<UmbMockMemberTypeModel> = rawData.map((mt) => ({
	...mt,
	compositions: mt.compositions.map((c) => ({
		...c,
		compositionType: mapCompositionType(c.compositionType),
	})),
}));
