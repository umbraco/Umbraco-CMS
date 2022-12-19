import { UmbEntityData } from './entity.data';
import type { UserGroupDetails } from '@umbraco-cms/models';

// Temp mocked database
class UmbUserGroupsData extends UmbEntityData<UserGroupDetails> {
	constructor(data: Array<UserGroupDetails>) {
		super(data);
	}

	getAll() {
		return this.data;
	}
}

export const data: Array<UserGroupDetails> = [
	{
		key: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
		parentKey: '',
		type: 'user-group',
		hasChildren: false,
		sections: [
			'Umb.Section.Users',
			'Umb.Section.Packages',
			'Umb.Section.Settings',
			'Umb.Section.Members',
			'Umb.Section.Media',
			'Umb.Section.Content',
		],
		permissions: [],
	},
	{
		key: '9a9ad4e9-3b5b-4fe7-b0d9-e301b9675949',
		name: 'Workspaces',
		icon: 'umb:tools',
		parentKey: '',
		type: 'user-group',
		hasChildren: false,
		sections: ['Umb.Section.Members', 'Umb.Section.Media'],
		permissions: [],
		contentStartNode: '74e4008a-ea4f-4793-b924-15e02fd380d1',
	},
	{
		key: 'b847398a-6875-4d7a-9f6d-231256b81471',
		name: 'Sensitive Data',
		icon: 'umb:lock',
		parentKey: '',
		type: 'user-group',
		hasChildren: false,
		sections: ['Umb.Section.Settings', 'Umb.Section.Members', 'Umb.Section.Media', 'Umb.Section.Content'],
		permissions: [],
		contentStartNode: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
	},
	{
		key: '2668f09b-320c-48a7-a78a-95047026ec0e',
		name: 'Translators',
		icon: 'umb:globe',
		parentKey: '',
		type: 'user-group',
		hasChildren: false,
		sections: ['Umb.Section.Packages', 'Umb.Section.Settings'],
		permissions: [],
		contentStartNode: 'cdd30288-2d1c-41b4-89a9-61647b4a10d5',
	},
	{
		key: '397f3a8b-4ca3-4b01-9dd3-94e5c9eaa9b2',
		name: 'Writers',
		icon: 'umb:edit',
		parentKey: '',
		type: 'user-group',
		hasChildren: false,
		sections: ['Umb.Section.Content'],
		permissions: [],
	},
];

export const umbUserGroupsData = new UmbUserGroupsData(data);
