import type { MemberResponseModel, MemberItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export type UmbMockMemberModel = MemberResponseModel & MemberItemResponseModel;

export const data: Array<UmbMockMemberModel> = [
	{
		email: 'member1@member.com',
		failedPasswordAttempts: 0,
		groups: [],
		id: '6ff6f75a-c14e-4172-a80b-d3ffcbc37979',
		isApproved: true,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		lastLockoutDate: null,
		lastLoginDate: null,
		lastPasswordChangeDate: null,
		memberType: { id: 'member-type-1-id', icon: '' },
		username: 'member1',
		values: [],
		variants: [
			{
				culture: 'en-us',
				segment: null,
				name: 'The Simplest Member',
				createDate: '2023-02-06T15:32:05.350038',
				updateDate: '2023-02-06T15:32:24.957009',
			},
		],
	},
	{
		email: 'member2@member.com',
		failedPasswordAttempts: 0,
		groups: [],
		id: '6ff6f75a-c14e-4172-a80b-d3ffcbc37979',
		isApproved: true,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		lastLockoutDate: null,
		lastLoginDate: null,
		lastPasswordChangeDate: null,
		memberType: { id: 'member-type-1-id', icon: '' },
		username: 'member2',
		values: [],
		variants: [
			{
				alias: 'prop1',
				culture: null,
				segment: null,
				value: 'default value here',
			},
		],
	},
	{
		email: 'member3@member.com',
		failedPasswordAttempts: 0,
		groups: [],
		id: '6ff6f75a-c14e-4172-a80b-d3ffcbc37979',
		isApproved: false,
		isLockedOut: false,
		isTwoFactorEnabled: false,
		lastLockoutDate: null,
		lastLoginDate: null,
		lastPasswordChangeDate: null,
		memberType: { id: 'member-type-1-id', icon: '' },
		username: 'member3',
		values: [],
		variants: [
			{
				name: 'Member 3',
				culture: 'en-us',
				segment: null,
				createDate: '2023-02-06T15:31:46.876902',
				updateDate: '2023-02-06T15:31:51.354764',
			},
		],
	},
];
