import type { ManifestRepository, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_SECTION_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.Section.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_SECTION_ITEM_REPOSITORY_ALIAS,
	name: 'Section Item Repository',
	api: () => import('./section-item.repository.js'),
};

export const manifests: Array<ManifestTypes> = [itemRepository];
