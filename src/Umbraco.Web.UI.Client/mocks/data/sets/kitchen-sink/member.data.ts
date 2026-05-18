import type { UmbMockMemberModel } from '../../mock-data-set.types.js';
import { MemberKindModel } from '@umbraco-cms/backoffice/external/backend-api';

const rawData = [
	{
		id: 'e93b2557-5fcb-4495-bbb3-9f5fd87055a8',
		email: 'member-one@umbraco.com',
		username: 'member-one@umbraco.com',
		isApproved: true,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		failedPasswordAttempts: 0,
		lastLoginDate: null,
		lastLockoutDate: null,
		lastPasswordChangeDate: '2023-02-20 15:37:48',
		memberType: {
			id: 'd59be02f-1df9-4228-aa1e-01917d806cda',
			icon: 'icon-user',
		},
		groups: ['4bff0fe9-6cf4-47cd-a87e-cd4a3a860c86'],
		kind: 'Default',
		values: [
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'umbracoMemberComments',
				value: '',
			},
		],
		variants: [
			{
				culture: null,
				segment: null,
				name: 'Member One',
				createDate: '2023-02-20 15:37:49',
				updateDate: '2023-02-20 15:37:49',
			},
		],
		flags: [],
	},
	{
		id: 'd74d2bd0-f55a-4a06-beb8-d8e931fc726b',
		email: 'member-two@umbraco.com',
		username: 'member-two@umbraco.com',
		isApproved: true,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		failedPasswordAttempts: 0,
		lastLoginDate: null,
		lastLockoutDate: null,
		lastPasswordChangeDate: '2023-02-20 15:38:20',
		memberType: {
			id: 'd59be02f-1df9-4228-aa1e-01917d806cda',
			icon: 'icon-user',
		},
		groups: ['015dd839-aace-4372-8238-5ec353c3a4d7'],
		kind: 'Default',
		values: [
			{
				editorAlias: 'Umbraco.TextArea',
				alias: 'umbracoMemberComments',
				value: '',
			},
		],
		variants: [
			{
				culture: null,
				segment: null,
				name: 'Member Two',
				createDate: '2023-02-20 15:38:20',
				updateDate: '2023-02-20 15:38:20',
			},
		],
		flags: [],
	},
];

export const data: Array<UmbMockMemberModel> = rawData.map((member) => ({
	...member,
	kind: MemberKindModel.DEFAULT,
}));
