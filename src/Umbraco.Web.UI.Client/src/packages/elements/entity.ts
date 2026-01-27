export const UMB_ELEMENT_ENTITY_TYPE = 'element';
export const UMB_ELEMENT_ROOT_ENTITY_TYPE = 'element-root';
export const UMB_ELEMENT_FOLDER_ENTITY_TYPE = 'element-folder';

export type UmbElementEntityType = typeof UMB_ELEMENT_ENTITY_TYPE;
export type UmbElementRootEntityType = typeof UMB_ELEMENT_ROOT_ENTITY_TYPE;
export type UmbElementFolderEntityType = typeof UMB_ELEMENT_FOLDER_ENTITY_TYPE;
export type UmbElementEntityTypeUnion = UmbElementEntityType | UmbElementRootEntityType | UmbElementFolderEntityType;

export const UMB_ELEMENT_PROPERTY_VALUE_ENTITY_TYPE = `${UMB_ELEMENT_ENTITY_TYPE}-property-value`;
export type UmbElementPropertyValueEntityType = typeof UMB_ELEMENT_PROPERTY_VALUE_ENTITY_TYPE;
