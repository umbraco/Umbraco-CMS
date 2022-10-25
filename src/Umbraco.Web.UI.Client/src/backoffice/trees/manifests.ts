import type { ManifestTree, ManifestWithLoader } from '@umbraco-cms/models';

export const manifests: Array<ManifestWithLoader<ManifestTree>> = [
	{
		type: 'tree',
		alias: 'Umb.Tree.Extensions',
		name: 'Extensions Tree',
		loader: () => import('./extensions/tree-extensions.element'),
		weight: 400,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DataTypes',
		name: 'Data Types Tree',
		loader: () => import('./data-types/tree-data-types.element'),
		weight: 300,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'Document Types Tree',
		loader: () => import('./document-types/tree-document-types.element'),
		weight: 200,
		meta: {
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Members',
		name: 'Members Tree',
		loader: () => import('./members/tree-members.element'),
		weight: 0,
		meta: {
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MemberGroups',
		name: 'Members Groups Tree',
		loader: () => import('./member-groups/tree-member-groups.element'),
		weight: 1,
		meta: {
			sections: ['Umb.Section.Members'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Media',
		name: 'Media Tree',
		loader: () => import('./media/tree-media.element'),
		weight: 100,
		meta: {
			sections: ['Umb.Section.Media'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.Documents',
		name: 'Documents Tree',
		loader: () => import('./documents/tree-documents.element'),
		weight: 100,
		meta: {
			sections: ['Umb.Section.Content'],
		},
	},
];
