export const UMB_SECTION_ALIASES_VALUE_TYPE = 'Umb.ValueType.UserGroup.SectionAliases' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_SECTION_ALIASES_VALUE_TYPE]: string[];
	}
}
