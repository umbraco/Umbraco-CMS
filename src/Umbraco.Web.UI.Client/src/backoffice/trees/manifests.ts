import type { ManifestTree } from '@umbraco-cms/models';


export const manifests: Array<ManifestTree> = [
	{
		type: 'tree',
		alias: 'Umb.Tree.Extensions',
		name: 'Extensions Tree',
		loader: () => import('./extensions/tree-extensions.element'),
		weight: 500,
		meta: {
			label: 'Extensions',
			icon: 'umb:favorite',
			sections: ['Umb.Section.Settings'],
			rootNodeEntityType: 'extensions-list', // TODO: how do we want to handle 'single node trees'. Trees without any children but still needs to open an workspace? Currently an workspace is chosen based on the entity type. The tree root node doesn't have one, so we need to tell which workspace to use.
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DocumentTypes',
		name: 'Document Types Tree',
		loader: () => import('./document-types/tree-document-types.element'),
		weight: 400,
		meta: {
			label: 'Document Types',
			icon: 'umb:folder',
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MediaTypes',
		name: 'Media Types Tree',
		loader: () => import('./media-types/tree-media-types.element'),
		weight: 300,
		meta: {
			label: 'Media Types',
			icon: 'umb:folder',
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MemberTypes',
		name: 'Member Types Tree',
		loader: () => import('./member-types/tree-member-types.element'),
		weight: 200,
		meta: {
			label: 'Member Types',
			icon: 'umb:folder',
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.DataTypes',
		name: 'Data Types Tree',
		loader: () => import('./data-types/tree-data-types.element'),
		weight: 100,
		meta: {
			label: 'Data Types',
			icon: 'umb:folder',
			sections: ['Umb.Section.Settings'],
		},
	},
	{
		type: 'tree',
		alias: 'Umb.Tree.MemberGroups',
		name: 'Member Groups Tree',
		loader: () => import('./member-groups/tree-member-groups.element'),
		weight: 1,
		meta: {
			label: 'Member Groups',
			icon: 'umb:folder',
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
			label: 'Media',
			icon: 'umb:folder',
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
			label: 'Documents',
			icon: 'umb:folder',
			sections: ['Umb.Section.Content'],
		},
	},
];
