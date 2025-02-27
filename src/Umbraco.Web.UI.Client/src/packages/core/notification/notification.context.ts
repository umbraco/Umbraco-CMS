import { UmbNotificationHandler } from './notification-handler.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';

/**
 * The default data of notifications
 * @interface UmbNotificationDefaultData
 */
export interface UmbNotificationDefaultData {
	message: string;
	headline?: string;
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

export class UmbNotificationContext extends UmbContextBase<UmbNotificationContext> {
	// Notice this cannot use UniqueBehaviorSubject as it holds a HTML Element. which cannot be Serialized to JSON (it has some circular references)
	private _notifications = new UmbBasicState(<Array<UmbNotificationHandler>>[]);
	public readonly notifications = this._notifications.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_NOTIFICATION_CONTEXT);
	}

	/**
	 * @private
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @returns {*}  {UmbNotificationHandler}
	 * @memberof UmbNotificationContext
	 */
	private _open(options: UmbNotificationOptions): UmbNotificationHandler {
		const notificationHandler = new UmbNotificationHandler(options);
		notificationHandler.element.addEventListener('closed', () => this._handleClosed(notificationHandler));

		this._notifications.setValue([...this._notifications.getValue(), notificationHandler]);

		return notificationHandler;
	}

	/**
	 * @private
	 * @param {string} key
	 * @memberof UmbNotificationContext
	 */
	private _close(key: string) {
		this._notifications.setValue(this._notifications.getValue().filter((notification) => notification.key !== key));
	}

	/**
	 * @private
	 * @param notificationHandler
	 * @param {string} key
	 * @memberof UmbNotificationContext
	 */
	private _handleClosed(notificationHandler: UmbNotificationHandler) {
		notificationHandler.element.removeEventListener('closed', () => this._handleClosed(notificationHandler));
		this._close(notificationHandler.key);
	}

	/**
	 * Opens a notification that automatically goes away after 6 sek.
	 * @param {UmbNotificationColor} color
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @returns {*}
	 * @memberof UmbNotificationContext
	 */
	public peek(color: UmbNotificationColor, options: UmbNotificationOptions): UmbNotificationHandler {
		return this._open({ color, ...options });
	}

	/**
	 * Opens a notification that stays on the screen until dismissed by the user or custom code
	 * @param {UmbNotificationColor} color
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @returns {*}
	 * @memberof UmbNotificationContext
	 */
	public stay(color: UmbNotificationColor, options: UmbNotificationOptions): UmbNotificationHandler {
		return this._open({ ...options, color, duration: null });
	}
}

export const UMB_NOTIFICATION_CONTEXT = new UmbContextToken<UmbNotificationContext>('UmbNotificationContext');
