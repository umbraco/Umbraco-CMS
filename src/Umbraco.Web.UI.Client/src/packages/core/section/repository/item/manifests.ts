import UmbSectionItemRepository from './section-item.repository.js';
export const UMB_SECTION_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Section.Item';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SECTION_ITEM_REPOSITORY_ALIAS,
		name: 'Section Item Repository',
		api: UmbSectionItemRepository,
	},
];
