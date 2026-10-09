import type { UmbMockTrackedReferenceItemModel } from '../../mock-data-set.types.js';

export const items: Array<UmbMockTrackedReferenceItemModel> = [
	{ $type: 'DocumentReferenceResponseModel', id: 'simple-document-id' },
	{ $type: 'DocumentReferenceResponseModel', id: 'block-editors-document-id' },
	{ $type: 'MediaReferenceResponseModel', id: 'f2f81a40-c989-4b6b-84e2-057cecd3adc1' },
	{
		$type: 'DefaultReferenceResponseModel',
		id: 'default-id',
		name: 'Some other reference',
		type: 'Default',
		icon: 'icon-bug',
	},
];
