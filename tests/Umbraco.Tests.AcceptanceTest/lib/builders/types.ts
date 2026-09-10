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
