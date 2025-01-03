export const UMB_MEDIA_ENTITY_TYPE = 'media';
export const UMB_MEDIA_ROOT_ENTITY_TYPE = 'media-root';
export const UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE = 'umb-media-placeholder';

export type UmbMediaEntityType = typeof UMB_MEDIA_ENTITY_TYPE;
export type UmbMediaRootEntityType = typeof UMB_MEDIA_ROOT_ENTITY_TYPE;

export type UmbMediaPlaceholderEntityType = typeof UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE;

export type UmbMediaEntityTypeUnion = UmbMediaEntityType | UmbMediaRootEntityType;
