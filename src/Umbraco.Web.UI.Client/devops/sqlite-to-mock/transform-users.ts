/**
 * Transform umbracoUser and umbracoUserGroup records into mock data format.
 */
import { prepare, formatGuid, writeDataFile, type User, type UserGroup } from './db.js';

interface UserGroupMembership {
	userId: number;
	userGroupId: number;
}

interface UserGroupPermission {
	id: number;
	userGroupKey: string;
	permission: string;
}

interface UserStartNode {
	userId: number;
	startNode: string;
	startNodeType: number;
}

export function transformUsers(): void {
	// Query users
	const userQuery = prepare(`
		SELECT
			id, userDisabled, userNoConsole, userName, userLogin, userPassword,
			passwordConfig, userEmail, userLanguage, createDate, updateDate,
			avatar, failedLoginAttempts, lastLockoutDate, lastPasswordChangeDate,
			lastLoginDate, emailConfirmedDate, invitedDate, securityStampToken, key
		FROM umbracoUser
	`);

	const users = userQuery.all() as User[];

	// Query user groups
	const groupQuery = prepare(`
		SELECT
			id, userGroupAlias, userGroupName, userGroupDefaultPermissions,
			createDate, updateDate, icon, hasAccessToAllLanguages,
			startContentId, startMediaId, key
		FROM umbracoUserGroup
	`);

	const groups = groupQuery.all() as UserGroup[];

	// Query user group memberships
	const membershipQuery = prepare(`
		SELECT userId, userGroupId
		FROM umbracoUser2UserGroup
	`);

	const memberships = membershipQuery.all() as UserGroupMembership[];

	// Query user group permissions
	const permissionQuery = prepare(`
		SELECT id, userGroupKey, permission
		FROM umbracoUserGroup2Permission
	`);

	const permissions = permissionQuery.all() as UserGroupPermission[];

	// Query user start nodes
	const startNodeQuery = prepare(`
		SELECT userId, startNode, startNodeType
		FROM umbracoUserStartNode
	`);

	const startNodes = startNodeQuery.all() as UserStartNode[];

	// Build lookup maps
	const groupIdMap = new Map<number, string>();
	for (const g of groups) {
		groupIdMap.set(g.id, formatGuid(g.key));
	}

	const membershipsByUser = new Map<number, number[]>();
	for (const m of memberships) {
		const list = membershipsByUser.get(m.userId) || [];
		list.push(m.userGroupId);
		membershipsByUser.set(m.userId, list);
	}

	const permissionsByGroup = new Map<string, UserGroupPermission[]>();
	for (const p of permissions) {
		const key = p.userGroupKey.toLowerCase();
		const list = permissionsByGroup.get(key) || [];
		list.push(p);
		permissionsByGroup.set(key, list);
	}

	const startNodesByUser = new Map<number, UserStartNode[]>();
	for (const sn of startNodes) {
		const list = startNodesByUser.get(sn.userId) || [];
		list.push(sn);
		startNodesByUser.set(sn.userId, list);
	}

	// Helper to determine user state
	function getUserState(user: User): string {
		if (user.userDisabled === 1) return 'Disabled';
		if (user.lastLockoutDate && !user.lastLoginDate) return 'LockedOut';
		if (user.invitedDate && !user.lastLoginDate) return 'Invited';
		if (!user.lastLoginDate) return 'Inactive';
		return 'Active';
	}

	// Transform users
	const transformedUsers = users.map((user) => {
		const userGroupIds = membershipsByUser.get(user.id) || [];
		const userStartNodes = startNodesByUser.get(user.id) || [];

		// Separate document and media start nodes
		// startNodeType: 1 = Content, 2 = Media
		const documentStartNodeIds = userStartNodes
			.filter((sn) => sn.startNodeType === 1)
			.map((sn) => ({ id: formatGuid(sn.startNode) }));

		const mediaStartNodeIds = userStartNodes
			.filter((sn) => sn.startNodeType === 2)
			.map((sn) => ({ id: formatGuid(sn.startNode) }));

		// Check if user is admin (member of Administrators group)
		const adminGroup = groups.find((g) => g.userGroupAlias === 'admin');
		const isAdmin = adminGroup ? userGroupIds.includes(adminGroup.id) : false;

		return {
			avatarUrls: [] as string[],
			createDate: user.createDate,
			documentStartNodeIds,
			email: user.userEmail,
			failedLoginAttempts: Number(user.failedLoginAttempts) || 0,
			hasDocumentRootAccess: documentStartNodeIds.length === 0,
			hasMediaRootAccess: mediaStartNodeIds.length === 0,
			id: formatGuid(user.key),
			isAdmin,
			kind: 'Default',
			languageIsoCode: user.userLanguage || 'en-us',
			lastLockoutDate: user.lastLockoutDate,
			lastLoginDate: user.lastLoginDate,
			lastPasswordChangeDate: user.lastPasswordChangeDate,
			mediaStartNodeIds,
			name: user.userName,
			state: getUserState(user),
			updateDate: user.updateDate,
			userGroupIds: userGroupIds.map((id) => ({ id: groupIdMap.get(id) || `unknown-${id}` })),
			userName: user.userLogin,
			flags: [] as string[],
		};
	});

	// Transform user groups
	const transformedGroups = groups.map((group) => {
		const groupKey = formatGuid(group.key);
		const groupPermissions = permissionsByGroup.get(groupKey.toLowerCase()) || [];

		// Map old single-letter permissions to new permission verbs
		const legacyPermissionMap: Record<string, string> = {
			F: 'Umb.Document.Read',
			C: 'Umb.Document.Create',
			A: 'Umb.Document.Update',
			D: 'Umb.Document.Delete',
			K: 'Umb.Document.CreateBlueprint',
			N: 'Umb.Document.Notifications',
			U: 'Umb.Document.Publish',
			P: 'Umb.Document.Permissions',
			Z: 'Umb.Document.Unpublish',
			O: 'Umb.Document.Duplicate',
			M: 'Umb.Document.Move',
			S: 'Umb.Document.Sort',
			H: 'Umb.Document.CultureAndHostnames',
			I: 'Umb.Document.PublicAccess',
			R: 'Umb.Document.Rollback',
		};

		// Build fallbackPermissions from umbracoUserGroup2Permission table
		// Filter out legacy single-character codes and map them to new format
		const fallbackPermissions = [
			...new Set(
				groupPermissions
					.map((p) => {
						// If it's a new-style permission (starts with "Umb."), use it directly
						if (p.permission.startsWith('Umb.')) {
							return p.permission;
						}
						// If it's a single-letter legacy permission, map it
						if (p.permission.length === 1 && legacyPermissionMap[p.permission]) {
							return legacyPermissionMap[p.permission];
						}
						// Skip unknown permissions
						return null;
					})
					.filter((p): p is string => p !== null),
			),
		];

		// Transform granular permissions - use fallback permissions instead since nodeKey doesn't exist
		const granularPermissions: Array<{ $type: string; document: { id: string }; verbs: string[] }> = [];

		// Determine sections based on group alias
		const sectionMap: Record<string, string[]> = {
			admin: [
				'Umb.Section.Content',
				'Umb.Section.Media',
				'Umb.Section.Settings',
				'Umb.Section.Members',
				'Umb.Section.Packages',
				'Umb.Section.Translation',
				'Umb.Section.Users',
			],
			editor: ['Umb.Section.Content', 'Umb.Section.Media'],
			writer: ['Umb.Section.Content'],
			translator: ['Umb.Section.Translation'],
			sensitiveData: [],
		};

		const sections = sectionMap[group.userGroupAlias] || ['Umb.Section.Content'];

		return {
			id: groupKey,
			name: group.userGroupName,
			alias: group.userGroupAlias,
			description: null as string | null,
			icon: group.icon || 'icon-users',
			fallbackPermissions,
			permissions: granularPermissions,
			sections,
			languages: [] as string[],
			hasAccessToAllLanguages: group.hasAccessToAllLanguages === 1,
			documentRootAccess: !group.startContentId,
			mediaRootAccess: !group.startMediaId,
			documentStartNode: group.startContentId ? { id: formatGuid(String(group.startContentId)) } : undefined,
			mediaStartNode: group.startMediaId ? { id: formatGuid(String(group.startMediaId)) } : undefined,
			aliasCanBeChanged: group.userGroupAlias !== 'admin' && group.userGroupAlias !== 'sensitiveData',
			isDeletable: group.userGroupAlias !== 'admin' && group.userGroupAlias !== 'sensitiveData',
			flags: [] as string[],
		};
	});

	// Generate user TypeScript content
	const userContent = `import type { UmbMockUserModel } from '../../types/mock-data-set.types.js';
import { UserKindModel, UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

// Map string state to enum
function mapState(state: string): UserStateModel {
	switch (state) {
		case 'Active': return UserStateModel.ACTIVE;
		case 'Disabled': return UserStateModel.DISABLED;
		case 'LockedOut': return UserStateModel.LOCKED_OUT;
		case 'Invited': return UserStateModel.INVITED;
		case 'Inactive': return UserStateModel.INACTIVE;
		default: return UserStateModel.ACTIVE;
	}
}

const rawData = ${JSON.stringify(transformedUsers, null, '\t')};

export const data: Array<UmbMockUserModel> = rawData.map(user => ({
	...user,
	kind: UserKindModel.DEFAULT,
	state: mapState(user.state),
}));
`;

	writeDataFile('user.data.ts', userContent);
	console.log(`Transformed ${transformedUsers.length} users`);

	// Generate user group TypeScript content
	const groupContent = `import type { UmbMockUserGroupModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockUserGroupModel> = ${JSON.stringify(transformedGroups, null, '\t')};
`;

	writeDataFile('user-group.data.ts', groupContent);
	console.log(`Transformed ${transformedGroups.length} user groups`);
}

// Run if called directly
transformUsers();
