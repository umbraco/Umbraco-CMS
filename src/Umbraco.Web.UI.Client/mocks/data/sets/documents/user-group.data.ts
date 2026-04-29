import type { UmbMockUserGroupModel } from '../../mock-data-set.types.js';

export const ADMIN_USER_GROUP_ID = 'variant-documents-user-group-administrators-id';

export const data: Array<UmbMockUserGroupModel> = [
	{
		id: ADMIN_USER_GROUP_ID,
		name: 'Administrators',
		alias: 'admin',
		description: 'Administrators have full access to all settings and features within the CMS.',
		icon: 'icon-medal',
		fallbackPermissions: [
			'Umb.Document.Read',
			'Umb.Document.Create',
			'Umb.Document.Update',
			'Umb.Document.Delete',
			'Umb.Document.CreateBlueprint',
			'Umb.Document.Notifications',
			'Umb.Document.Publish',
			'Umb.Document.Permissions',
			'Umb.Document.Unpublish',
			'Umb.Document.Duplicate',
			'Umb.Document.Move',
			'Umb.Document.Sort',
			'Umb.Document.CultureAndHostnames',
			'Umb.Document.PublicAccess',
			'Umb.Document.Rollback',
			'Umb.Document.PropertyValue.Read',
			'Umb.Document.PropertyValue.Write',
		],
		permissions: [],
		sections: [
			'Umb.Section.Content',
			'Umb.Section.Media',
			'Umb.Section.Settings',
			'Umb.Section.Members',
			'Umb.Section.Packages',
			'Umb.Section.Translation',
			'Umb.Section.Users',
		],
		languages: ['en-US', 'da'],
		hasAccessToAllLanguages: false,
		documentRootAccess: true,
		mediaRootAccess: true,
		aliasCanBeChanged: false,
		isDeletable: false,
		flags: [],
	},
];
