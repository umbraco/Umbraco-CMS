import { UMB_STYLESHEET_FOLDER_ENTITY_TYPE, UMB_STYLESHEET_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS } from '../../tree/folder/repository/index.js';

/** @deprecated No longer used internally. This will be removed in Umbraco 18. [LK] */
const modal: UmbExtensionManifest = {
	type: 'modal',
	alias: 'Umb.Modal.Stylesheet.CreateOptions',
	name: 'Stylesheet Create Options Modal',
	element: () => import('./options-modal/stylesheet-create-options-modal.element.js'),
};

const entityAction: UmbExtensionManifest = {
	type: 'entityAction',
	kind: 'create',
	alias: 'Umb.EntityAction.Stylesheet.Create',
	name: 'Create Stylesheet Entity Action',
	weight: 1200,
	forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
	meta: {
		icon: 'icon-add',
		label: '#actions_create',
		additionalOptions: true,
		headline: '#create_createUnder #treeHeaders_documentTypes',
	},
};

const entityCreateOptionActions: Array<UmbExtensionManifest> = [
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.Stylesheet.Default',
		name: 'Default Stylesheet Entity Create Option Action',
		weight: 100,
		api: () => import('./stylesheet-create-option-action.js'),
		forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-palette',
			label: '#create_newStyleSheetFile',
		},
	},
	{
		type: 'entityCreateOptionAction',
		kind: 'folder',
		alias: 'Umb.EntityCreateOptionAction.Stylesheet.Folder',
		name: 'Stylesheet Folder Entity Create Option Action',
		forEntityTypes: [UMB_STYLESHEET_ROOT_ENTITY_TYPE, UMB_STYLESHEET_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			description: '#create_folderDescription',
			folderRepositoryAlias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [modal, entityAction, ...entityCreateOptionActions];
