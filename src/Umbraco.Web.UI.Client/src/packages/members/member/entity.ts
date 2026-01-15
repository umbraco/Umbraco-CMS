export const UMB_MEMBER_ENTITY_TYPE = 'member';
export const UMB_MEMBER_ROOT_ENTITY_TYPE = 'member-root';

export type UmbMemberEntityType = typeof UMB_MEMBER_ENTITY_TYPE;
export type UmbMemberRootEntityType = typeof UMB_MEMBER_ROOT_ENTITY_TYPE;

// TODO: move this to a better location inside the member module
export const UMB_MEMBER_PROPERTY_VALUE_ENTITY_TYPE = `${UMB_MEMBER_ENTITY_TYPE}-property-value`;
export type UmbMemberPropertyValueEntityType = typeof UMB_MEMBER_PROPERTY_VALUE_ENTITY_TYPE;
