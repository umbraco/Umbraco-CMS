import { rest } from 'msw';
import type { UserGroupDetails } from '../../core/models';

export const handlers = [
	rest.get('/umbraco/backoffice/user-groups/list/items', (req, res, ctx) => {
		const items = fakeData;

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];

const fakeData: Array<UserGroupDetails> = [
	{
		key: '10000000-0000-0000-0000-000000000000',
		name: 'Administrators',
		icon: 'umb:medal',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
	},
	{
		key: '20000000-0000-0000-0000-000000000000',
		name: 'Editors',
		icon: 'umb:tools',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
	},
	{
		key: '20000000-0000-0000-0000-000000000000',
		name: 'Sensitive Data',
		icon: 'umb:lock',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
	},
	{
		key: '20000000-0000-0000-0000-000000000000',
		name: 'Translators',
		icon: 'umb:globe',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
	},
	{
		key: '20000000-0000-0000-0000-000000000000',
		name: 'Writers',
		icon: 'umb:edit',
		parentKey: '',
		type: 'userGroup',
		hasChildren: false,
		isTrashed: false,
	},
];
