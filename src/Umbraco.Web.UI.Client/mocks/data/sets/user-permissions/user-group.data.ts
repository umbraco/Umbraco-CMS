import type { UmbMockUserGroupModel } from '../../types/mock-data-set.types.js';

export const data: Array<UmbMockUserGroupModel> = [
	{
		id: 'user-group-administrators-id',
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
			'my-permission',
		],
		permissions: [
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-document-id',
				},
				verbs: ['Umb.Document.Read'],
			},
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-2-document-id',
				},
				verbs: ['Umb.Document.Create', 'Umb.Document.Read'],
			},
			{
				$type: 'DocumentPermissionPresentationModel',
				document: {
					id: 'permissions-2-2-document-id',
				},
				verbs: ['Umb.Document.Delete', 'Umb.Document.Read'],
			},
		],
		sections: [
			'Umb.Section.Content',
			'Umb.Section.Media',
			'Umb.Section.Settings',
			'Umb.Section.Members',
			'Umb.Section.Packages',
			'Umb.Section.Translation',
			'Umb.Section.Users',
		],
		languages: [],
		hasAccessToAllLanguages: true,
		documentRootAccess: true,
		mediaRootAccess: true,
		aliasCanBeChanged: false,
		isDeletable: false,
		flags: [],
	},
];
