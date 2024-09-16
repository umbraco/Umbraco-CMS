export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'section',
		alias: 'Umb.Section.Members',
		name: 'Members Section',
		weight: 500,
		meta: {
			label: '#sections_member',
			pathname: 'member-management',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionUserPermission',
				match: 'Umb.Section.Members',
			},
		],
	},
];
