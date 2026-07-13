import type { UmbMockUserGroupModel } from '../../mock-data-set.types.js';

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
			'Umb.Element.Read',
			'Umb.Element.Create',
			'Umb.Element.Update',
			'Umb.Element.Delete',
			'Umb.Element.Publish',
			'Umb.Element.Unpublish',
			'Umb.Element.Duplicate',
			'Umb.Element.Move',
			'Umb.Element.Rollback',
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
			{
				$type: 'ElementPermissionPresentationModel',
				element: { id: 'permissions-element-read-only-id' },
				verbs: ['Umb.Element.Read'],
			},
			// TODO: remove cast once ElementContainerPermissionPresentationModel is in the backend API [LK]
			{
				$type: 'ElementContainerPermissionPresentationModel',
				elementContainer: { id: 'permissions-element-read-only-id' },
				verbs: ['Umb.ElementContainer.Create'],
			} as unknown as UmbMockUserGroupModel['permissions'][number],
			{
				$type: 'ElementPermissionPresentationModel',
				element: { id: 'permissions-folder-read-only-id' },
				verbs: ['Umb.Element.Create'],
			},
			// TODO: remove cast once ElementContainerPermissionPresentationModel is in the backend API [LK]
			{
				$type: 'ElementContainerPermissionPresentationModel',
				elementContainer: { id: 'permissions-folder-read-only-id' },
				verbs: ['Umb.ElementContainer.Read'],
			} as unknown as UmbMockUserGroupModel['permissions'][number],
			{
				$type: 'ElementPermissionPresentationModel',
				element: { id: 'permissions-both-read-id' },
				verbs: ['Umb.Element.Read'],
			},
			// TODO: remove cast once ElementContainerPermissionPresentationModel is in the backend API [LK]
			{
				$type: 'ElementContainerPermissionPresentationModel',
				elementContainer: { id: 'permissions-both-read-id' },
				verbs: ['Umb.ElementContainer.Read'],
			} as unknown as UmbMockUserGroupModel['permissions'][number],
			{
				$type: 'ElementPermissionPresentationModel',
				element: { id: 'permissions-neither-read-id' },
				verbs: ['Umb.Element.Create'],
			},
			// TODO: remove cast once ElementContainerPermissionPresentationModel is in the backend API [LK]
			{
				$type: 'ElementContainerPermissionPresentationModel',
				elementContainer: { id: 'permissions-neither-read-id' },
				verbs: ['Umb.ElementContainer.Create'],
			} as unknown as UmbMockUserGroupModel['permissions'][number],
		],
		sections: [
			'Umb.Section.Content',
			'Umb.Section.Media',
			'Umb.Section.Library',
			'Umb.Section.Settings',
			'Umb.Section.Members',
			'Umb.Section.Packages',
			'Umb.Section.Translation',
			'Umb.Section.Users',
		],
		languages: [],
		hasAccessToAllLanguages: true,
		documentRootAccess: true,
		elementRootAccess: true,
		mediaRootAccess: true,
		aliasCanBeChanged: false,
		isDeletable: false,
		flags: [],
	},
];
