import {buildComposition, buildContainer, buildProperty} from '../helpers/BuilderUtils';

/**
 * Payload shapes the builders produce for the Management API.
 *
 * These type the *envelope* — the field names and nesting the API requires — rather than
 * every leaf value. A property `value` is genuinely heterogeneous (string, number, array,
 * nested block structure), so it stays `any`; what these catch is the mistake that actually
 * happens: a misspelled or missing envelope field, which otherwise compiles fine and comes
 * back as an opaque 400 at runtime.
 */

/** A single property value on an entity: `{alias, value}`. */
export interface EntityValue {
  alias: string;
  value?: any;
  culture?: string | null;
  segment?: string | null;
}

/** A reference to another entity by id, as the API expects it (`{id}`), or null for root. */
export interface EntityReference {
  id: string;
}

/**
 * A variant entry on a document, element, media, member or blueprint payload.
 *
 * `name` is nullable because most builders send `null` when it is unset; `MemberVariantBuilder`
 * sends `''` instead. That difference is deliberate here - it is the payload the suite has always
 * sent, and changing it wants verifying against a running instance rather than tidying by hand.
 */
export interface EntityVariant {
  culture: string | null;
  segment: string | null;
  name: string | null;
}

/**
 * A property value entry as the *builders* emit it, which is not the same shape as
 * {@link EntityValue}: an outgoing payload always carries `culture`, `segment` and `value`, and
 * its `alias` may be null, whereas a value read back off an entity is keyed by a required alias.
 *
 * `editorAlias` and `entityType` are optional because `MemberValueBuilder` sends neither.
 */
export interface EntityPropertyValue {
  culture: string | null;
  segment: string | null;
  alias: string | null;
  value: any;
  editorAlias?: string | null;
  entityType?: string | null;
}

/**
 * The container, property and composition shapes come from `BuilderUtils`, which already
 * declares them. Deriving them here keeps one definition: a change to `buildProperty`'s return
 * type reaches every content-type payload without a second place to update.
 */
export type ContentTypeContainer = ReturnType<typeof buildContainer>;
export type ContentTypeProperty = ReturnType<typeof buildProperty>;
export type ContentTypeComposition = ReturnType<typeof buildComposition>;

/**
 * An entry in `allowedDocumentTypes`. Kept separate from {@link AllowedMediaType} rather than
 * making both keys optional on one type, which would let a media entry satisfy a document-type
 * payload and vice versa.
 */
export interface AllowedDocumentType {
  documentType: { id: string | null };
  sortOrder: number;
}

/** An entry in `allowedMediaTypes`. */
export interface AllowedMediaType {
  mediaType: { id: string | null };
  sortOrder: number;
}

/** Fields shared by the document-type, media-type and member-type payloads. */
export interface ContentTypePayloadBase {
  alias: string;
  name: string;
  description: string;
  icon: string;
  allowedAsRoot: boolean;
  variesByCulture: boolean;
  variesBySegment: boolean;
  collection: EntityReference | null;
  isElement: boolean;
  properties: ContentTypeProperty[];
  containers: ContentTypeContainer[];
  compositions: ContentTypeComposition[];
  id: string;
  folder: EntityReference | null;
}

/** A document-type payload. */
export interface DocumentTypePayload extends ContentTypePayloadBase {
  allowedInLibrary: boolean;
  allowedDocumentTypes: AllowedDocumentType[];
  allowedTemplates: OptionalEntityReference[];
  defaultTemplate: EntityReference | null;
  cleanup: {
    preventCleanup: boolean;
    keepAllVersionsNewerThanDays: number | null;
    keepLatestVersionPerDayForDays: number | null;
  };
}

/** A media-type payload. */
export interface MediaTypePayload extends ContentTypePayloadBase {
  allowedMediaTypes: AllowedMediaType[];
}

/** A member-type payload. */
export interface MemberTypePayload extends ContentTypePayloadBase {
  allowedInLibrary: boolean;
}

/** Fields shared by every content-like entity payload the builders emit. */
export interface EntityPayloadBase {
  values: EntityPropertyValue[];
  variants: EntityVariant[];
  id: string | null;
}

/**
 * A document, document-blueprint or element payload. Elements are content types too, so they
 * carry `documentType` rather than a type of their own; `template` is document-only.
 */
export interface DocumentPayload extends EntityPayloadBase {
  parent: EntityReference | null;
  documentType: EntityReference;
  template?: EntityReference | null;
}

/** A media payload. */
export interface MediaPayload extends EntityPayloadBase {
  parent: EntityReference | null;
  mediaType: EntityReference;
}

/** A member payload. Members are not tree items, so there is no `parent`. */
export interface MemberPayload extends EntityPayloadBase {
  email: string;
  username: string;
  password: string;
  memberType: EntityReference;
  groups: string[];
  isApproved: boolean;
}

/**
 * A block editor's property value.
 *
 * The three arrays are parallel rather than nested: `contentData` holds each block's own
 * property values keyed by `key`, `layout` holds the arrangement referring back to those keys,
 * and `expose` marks which blocks are exposed for which culture/segment. A layout entry whose
 * `contentKey` matches no `contentData` key is accepted by the type system and rejected by the
 * API, so the keys are the part to check by eye.
 */

/** One `{alias, value}` entry inside a block's `contentData`. */
export interface BlockContentDataValue {
  alias: string;
  culture: string | null;
  editorAlias: string;
  segment: string | null;
  value: any;
}

/** An `expose` entry, marking a block's content as exposed for a culture/segment. */
export interface BlockExpose {
  contentKey: string;
  culture: string | null;
  segment: string | null;
}

/** A block-list `contentData` entry. */
export interface BlockListContentData {
  contentTypeKey: string;
  key: string;
  values: BlockContentDataValue[];
}

/** A block-grid `contentData` entry: the list's shape plus the legacy `udi`. */
export interface BlockGridContentData extends BlockListContentData {
  udi: string | null;
}

/** A block-list `layout` entry. */
export interface BlockListLayoutItem {
  contentKey: string;
}

/** A block-grid area, holding nested layout items. */
export interface BlockGridArea {
  key: string;
  items: BlockGridLayoutItem[];
}

/** A block-grid `layout` entry. Areas nest layout items, so this is mutually recursive. */
export interface BlockGridLayoutItem {
  $type: string;
  columnSpan: number;
  contentKey: string;
  areas: BlockGridArea[];
  contentUdi: string | null;
  rowSpan: number;
  settingsKey: string | null;
  settingsUdi: string | null;
}

/** The value a block-list property carries. The layout key is the editor alias. */
export interface BlockListValue {
  contentData: BlockListContentData[];
  expose: BlockExpose[];
  layout: { 'Umbraco.BlockList': BlockListLayoutItem[] };
  settingsData: any[];
}

/** The value a block-grid property carries. */
export interface BlockGridValue {
  contentData: BlockGridContentData[];
  expose: BlockExpose[];
  layout: { 'Umbraco.BlockGrid': BlockGridLayoutItem[] };
  settingsData: any[];
}

/**
 * User-group permissions. The API discriminates the three kinds on `$type`, so these are a
 * discriminated union rather than one type with every key optional - that way a document
 * permission cannot be handed to a property-value slot.
 */

/** The verbs a permission grants, e.g. `'Umb.Document.Read'`. */
export type UserGroupVerbs = string[];

/** A permission scoped to one document. */
export interface UserGroupDocumentPermission {
  $type: 'DocumentPermissionPresentationModel';
  document: EntityReference | null;
  verbs: UserGroupVerbs;
}

/** A permission scoped to one element. */
export interface UserGroupElementPermission {
  $type: 'ElementPermissionPresentationModel';
  element: EntityReference | null;
  verbs: UserGroupVerbs;
}

/** A permission scoped to one property on one document type. */
export interface UserGroupPropertyValuePermission {
  $type: 'DocumentPropertyValuePermissionPresentationModel';
  documentType: EntityReference | null;
  propertyType: EntityReference | null;
  verbs: UserGroupVerbs;
}

export type UserGroupPermission =
  | UserGroupDocumentPermission
  | UserGroupElementPermission
  | UserGroupPropertyValuePermission;

/** A user-group payload. */
export interface UserGroupPayload {
  name: string;
  alias: string;
  icon: string;
  sections: string[];
  languages: string[];
  hasAccessToAllLanguages: boolean;
  documentStartNode: EntityReference | null;
  documentRootAccess: boolean;
  mediaStartNode: EntityReference | null;
  mediaRootAccess: boolean;
  fallbackPermissions: UserGroupVerbs;
  permissions: UserGroupPermission[];
  description: string;
  elementStartNode: EntityReference | null;
  elementRootAccess: boolean;
}


/** A reference that may legitimately be unset, as an `allowedTemplates` entry is. */
export interface OptionalEntityReference {
  id: string | null;
}

/** A block-grid block group. */
export interface BlockGridBlockGroup {
  key: string;
  name: string | null;
}

/** The list-view bulk-action permission flags. */
export interface ListViewBulkActionPermissions {
  allowBulkCopy: boolean;
  allowBulkDelete: boolean;
  allowBulkMove: boolean;
  allowBulkPublish: boolean;
  allowBulkUnPublish: boolean;
}

/** One collection view offered by a list view. */
export interface ListViewLayout {
  collectionView: string;
  icon: string;
  name: string | null;
}

/** One column a list view shows. */
export interface ListViewProperty {
  alias: string;
  header: string;
  nameTemplate: string | null;
  isSystem: boolean;
}

/** One step in a multi-node-tree-picker start-node query. */
export interface StartNodeQueryStep {
  unique: string;
  alias: string;
  anyOfDocTypeKeys: string[];
}

/** One culture assignment on a document's domains. */
export interface DocumentDomainValue {
  domainName: string;
  isoCode: string;
}

/** The domains payload for a document. */
export interface DocumentDomainsPayload {
  domains: DocumentDomainValue[];
  defaultIsoCode: string | null;
}

/** A package definition payload. Each array holds the ids of the entities to include. */
export interface PackagePayload {
  name: string;
  contentNodeId: string;
  contentLoadChildNodes: boolean;
  mediaIds: string[];
  mediaLoadChildNodes: boolean;
  documentTypes: string[];
  mediaTypes: string[];
  dataTypes: string[];
  templates: string[];
  partialViews: string[];
  stylesheets: string[];
  scripts: string[];
  languages: string[];
  dictionaryItems: string[];
}

/** A user payload. */
export interface UserPayload {
  email: string;
  name: string;
  kind: string;
  userGroupIds: EntityReference[];
  userName: string;
}

/** A webhook payload. */
export interface WebhookPayload {
  enabled: boolean;
  name: string;
  description: string;
  url: string;
  contentTypeKeys: string[];
  headers: Record<string, string>;
  events: string[];
}


/**
 * Configuration and value shapes built up conditionally - a field is emitted only when set - so
 * every property is optional.
 *
 * Declaring the local as one of these is the part that matters, not the return type: assigning
 * `values.labell` to a typed object is an error naming the typo, while the same assignment on an
 * `any` local is silent. Bracket assignment (`values['labell']`) stays unchecked either way
 * because `noImplicitAny` is off, which is why these builders use dot access.
 */

/** A focal point on a cropped image. */
export interface FocalPoint {
  left: number;
  top: number;
}

/** An image value: the shape both the image cropper and a media property emit. */
export interface ImageValue {
  crops?: any[];
  focalPoint?: FocalPoint | null;
  src?: string;
  temporaryFileId?: string;
}

/** A media-picker property value entry. */
export interface MediaPickerValue {
  key?: string;
  mediaKey?: string;
  mediaTypeAlias?: string;
  focalPoint?: FocalPoint | null;
  crops?: any[];
}

/** A URL-picker property value entry. */
export interface UrlPickerValue {
  icon?: string;
  name?: string | null;
  published?: boolean;
  queryString?: string | null;
  target?: string | null;
  trashed?: boolean;
  type?: string;
  unique?: string | null;
  url?: string;
}

/** A crop definition on an image-cropper or media-picker data type. */
export interface CropConfiguration {
  label?: string;
  alias?: string;
  width?: number;
  height?: number;
}

/** An approved-colour item. */
export interface ApprovedColorItem {
  label?: string;
  value?: string;
}

/** One entry in a block-grid area's `specifiedAllowance`. */
export interface BlockGridSpecifiedAllowance {
  elementTypeKey?: string;
  groupKey?: string;
  minAllowed?: number;
  maxAllowed?: number;
}

/** A block-grid area's configuration. */
export interface BlockGridAreaConfiguration {
  key?: string;
  alias?: string;
  columnSpan?: number;
  rowSpan?: number;
  minAllowed?: number;
  maxAllowed?: number;
  createLabel?: string;
  specifiedAllowance?: BlockGridSpecifiedAllowance[];
}

/**
 * Configuration common to a block-list and a block-grid block. `stylesheet` is deliberately not
 * here: the block-list builder holds a `string[]` and the block-grid builder a `string`, so it is
 * declared per editor rather than unified, which would change one of the two payloads.
 */
export interface BlockConfigurationBase {
  contentElementTypeKey?: string;
  label?: string;
  settingsElementTypeKey?: string;
  editorSize?: string;
  backgroundColor?: string;
  iconColor?: string;
  thumbnail?: string;
}

/** A block-list block's configuration. */
export interface BlockListBlockConfiguration extends BlockConfigurationBase {
  stylesheet?: string[];
  forceHideContentEditorInOverlay?: boolean;
}

/** A block-grid block's configuration. */
export interface BlockGridBlockConfiguration extends BlockConfigurationBase {
  stylesheet?: string;
  allowAtRoot?: boolean;
  allowInAreas?: boolean;
  columnSpanOptions?: { columnSpan: number }[];
  rowMinSpan?: number;
  rowMaxSpan?: number;
  inlineEditing?: boolean;
  hideContentEditor?: boolean;
  view?: string;
  groupKey?: string;
  areas?: BlockGridAreaConfiguration[];
  areaGridColumns?: number;
}

/** A tiptap block's configuration. */
export interface TiptapBlockConfiguration {
  contentElementTypeKey?: string;
  displayInline?: boolean;
  backgroundColor?: string;
  iconColor?: string;
  thumbnail?: string;
  editorSize?: string;
  label?: string;
  settingsElementTypeKey?: string;
}

/** A multi-node-tree-picker start node. */
export interface MultiNodeTreePickerStartNode {
  type?: string;
  originAlias?: string;
  startNodeQuerySteps?: StartNodeQueryStep[];
}


/** What every `DataTypeBuilder.getValues()` returns. */
export type DataTypeValues = EntityValue[];

/** What `DataTypeBuilder.build()` returns. */
export interface DataTypePayload {
  editorAlias: string;
  editorUiAlias: string;
  id: string;
  name: string;
  parent: EntityReference | null;
  values: DataTypeValues;
}
