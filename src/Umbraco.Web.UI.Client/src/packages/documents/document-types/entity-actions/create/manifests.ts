import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../tree/index.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityAction',
		kind: 'create',
		alias: 'Umb.EntityAction.DocumentType.Create',
		name: 'Create Document Type Entity Action',
		weight: 1200,
		forEntityTypes: [UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-add',
			label: '#actions_create',
			additionalOptions: true,
			headline: '#create_createUnder #treeHeaders_documentTypes',
		},
	},
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.DocumentType.Default',
		name: 'Default Document Type Entity Create Option Action',
		weight: 100,
		api: () => import('./default/default-document-type-create-option-action.js'),
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
		meta: {
			icon: 'icon-document',
			label: '#create_documentType',
			description: '#create_documentTypeDescription',
		},
	},
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.DocumentType.DocumentWithTemplate',
		name: 'Document Type with Template Document Type Entity Create Option Action',
		weight: 90,
		api: () => import('./template/document-type-with-template-create-option-action.js'),
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
		meta: {
			icon: 'icon-document-html',
			label: '#create_documentTypeWithTemplate',
			description: '#create_documentTypeWithTemplateDescription',
		},
	},
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.DocumentType.ElementType',
		name: 'Element Type Document Type Entity Create Option Action',
		weight: 80,
		api: () => import('./element/element-type-create-option-action.js'),
		forEntityTypes: [
			UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		],
		meta: {
			icon: 'icon-plugin',
			label: '#create_elementType',
			description: '#create_elementTypeDescription',
		},
	},
	{
		type: 'entityCreateOptionAction',
		alias: 'Umb.EntityCreateOptionAction.DocumentType.Folder',
		name: 'Folder Document Type Entity Create Option Action',
		weight: 70,
		api: () => import('./folder/document-type-folder-create-option-action.js'),
		forEntityTypes: [UMB_DOCUMENT_TYPE_ROOT_ENTITY_TYPE, UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE],
		meta: {
			icon: 'icon-folder',
			label: '#create_folder',
			description: '#create_folderDescription',
		},
	},
];
