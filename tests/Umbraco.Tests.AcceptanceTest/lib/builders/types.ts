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
  allowedTemplates: { id: string | null }[];
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
