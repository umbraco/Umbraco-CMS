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
		key: '10000000-0000-0000-0000-000000000000',
		name: 'Administrators',
		icon: 'umb:medal',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
	},
	{
		key: '20000000-0000-0000-0000-000000000000',
		name: 'Editors',
		icon: 'umb:tools',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
	},
	{
		key: '30000000-0000-0000-0000-000000000000',
		name: 'Sensitive Data',
		icon: 'umb:lock',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
	},
	{
		key: '40000000-0000-0000-0000-000000000000',
		name: 'Translators',
		icon: 'umb:globe',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
	},
	{
		key: '50000000-0000-0000-0000-000000000000',
		name: 'Writers',
		icon: 'umb:edit',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
		sections: [],
	},
];

export const umbUserGroupsData = new UmbUserGroupsData(data);
