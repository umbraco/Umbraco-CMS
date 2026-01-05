import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../tree/folder/entity.js';
import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS } from '../../tree/folder/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.Element.Create',
		name: 'Create Element Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_createFor',
			additionalOptions: true,
			headline: '#create_createUnder #treeHeaders_elements',
		},
	},
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.Element.Default',
		name: 'Default Element Entity Create Option Action',
		weight: 100,
		api: () => import('./element-create-option-action.js'),
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-document',
			label: '#create_element',
			description: '#create_elementDescription',
		},
	},
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.Element.Folder',
		name: 'Element Folder Entity Create Option Action',
		forEntityTypes: [UMB_ELEMENT_ROOT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			additionalOptions: true,
			folderRepositoryAlias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		},
	},
];
