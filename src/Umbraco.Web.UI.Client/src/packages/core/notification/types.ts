/**
 * The default data of notifications
 * @interface UmbNotificationDefaultData
 */
export interface UmbNotificationDefaultData {
	message: string;
	headline?: string;
	/**
	 * @deprecated, do not use this. It will be removed in v.16 — Use UmbPeekError instead
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

export interface UmbPeekErrorArgs extends UmbNotificationDefaultData {
	details?: unknown;
	color?: UmbNotificationColor;
}
