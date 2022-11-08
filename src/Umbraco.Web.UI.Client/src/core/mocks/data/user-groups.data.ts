import { UmbEntityData } from './entity.data';
import type { UserGroupDetails } from '@umbraco-cms/models';

// Temp mocked database
class UmbUserGroupsData extends UmbEntityData<UserGroupDetails> {
	constructor(data: Array<UserGroupDetails>) {
		super(data);
	}
}

export const data: Array<UserGroupDetails> = [
	{
		key: 'c630d49e-4e7b-42ea-b2bc-edc0edacb6b1',
		name: 'Administrators',
		icon: 'umb:medal',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
		permissions: [],
	},
	{
		key: '9a9ad4e9-3b5b-4fe7-b0d9-e301b9675949',
		name: 'Editors',
		icon: 'umb:tools',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
		permissions: [],
	},
	{
		key: 'b847398a-6875-4d7a-9f6d-231256b81471',
		name: 'Sensitive Data',
		icon: 'umb:lock',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
		permissions: [],
	},
	{
		key: '2668f09b-320c-48a7-a78a-95047026ec0e',
		name: 'Translators',
		icon: 'umb:globe',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
		permissions: [],
	},
	{
		key: '397f3a8b-4ca3-4b01-9dd3-94e5c9eaa9b2',
		name: 'Writers',
		icon: 'umb:edit',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
		permissions: [],
	},
];

export const umbUserGroupsData = new UmbUserGroupsData(data);
