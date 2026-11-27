export interface UmbPermissionDenyReason {
	/** A localization key or text. */
	title: string;
	/** A localization key or text. */
	detail?: string;
}

/**
 * Allows or denies permission verbs, such as `Umb.Document.Update`, within the scope its type defines.
 */
export interface UmbPermission<MetaType = unknown> {
	/** The kind of permission. Defines `meta` and which evaluator interprets the permission. */
	type: string;
	meta?: MetaType;
	allow?: Array<string>;
	deny?: Array<string>;
	/** Why the verbs in `deny` are denied. */
	denyReason?: UmbPermissionDenyReason;
}

export interface UmbPermissionConfig {
	/** Every verb must be permitted. */
	allOf?: Array<string>;
	/** At least one verb must be permitted. */
	oneOf?: Array<string>;
}

export interface UmbPermissionResult {
	/** `undefined` until every evaluator involved has answered. */
	permitted: boolean | undefined;
	/** Why the result is denied. Empty unless `permitted` is `false`, and empty when nothing allows a verb. */
	denyReasons: Array<UmbPermissionDenyReason>;
}
