import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

/**
 * The default data of notifications
 * @interface UmbNotificationDefaultData
 */
export interface UmbNotificationDefaultData {
	/**
	 * The notification message, rendered as plain text. Use {@link UmbNotificationDefaultData.htmlMessage} to render HTML.
	 */
	message: string;
	/**
	 * A message rendered as HTML. Takes precedence over `message` when set.
	 *
	 * A string value is sanitized before rendering, so scripts and event handlers are stripped.
	 * A TemplateResult is rendered as-is — Lit escapes its bindings, so it is safe by construction
	 * unless you explicitly opt out with `unsafeHTML`.
	 *
	 * Do not interpolate user-controlled content into an HTML string: sanitization prevents script
	 * execution but not markup injection (e.g. links or images).
	 */
	htmlMessage?: string | TemplateResult;
	headline?: string;
	/**
	 * @deprecated Use {@link UmbPeekErrorArgs} instead. Scheduled for removal in Umbraco 19.
	 */
	structuredList?: Record<string, Array<unknown>>;
	whitespace?: 'normal' | 'pre-line' | 'pre-wrap' | 'nowrap' | 'pre';
}

/**
 * @interface UmbNotificationOptions
 * @template UmbNotificationData
 */
export interface UmbNotificationOptions<UmbNotificationData = UmbNotificationDefaultData> {
	color?: UmbNotificationColor;
	duration?: number | null;
	elementName?: string;
	data?: UmbNotificationData;
}

export type UmbNotificationColor = '' | 'default' | 'positive' | 'warning' | 'danger';

/**
 * Arguments for displaying an error peek notification.
 * @interface UmbPeekErrorArgs
 */
export interface UmbPeekErrorArgs extends UmbNotificationDefaultData {
	/** A human-readable explanation of the error (from ProblemDetails.detail). */
	detail?: string;
	/** Validation errors keyed by field name (from ProblemDetails.errors). */
	errors?: Record<string, string[]>;
	/**
	 * Validation errors keyed by field name (from ProblemDetails.errors).
	 * @deprecated Use `errors` instead. Scheduled for removal in Umbraco 19.
	 */
	details?: Record<string, string[]>;
	/** The notification color. Defaults to 'danger'. */
	color?: UmbNotificationColor;
}
