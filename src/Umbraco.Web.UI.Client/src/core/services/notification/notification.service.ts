import { BehaviorSubject, Observable } from 'rxjs';
import { UmbNotificationHandler } from './';

export type UmbNotificationData = any;

/**
 * @export
 * @interface UmbNotificationOptions
 * @template UmbNotificationData
 */
export interface UmbNotificationOptions<UmbNotificationData> {
	color?: UmbNotificationColor;
	duration?: number | null;
	elementName?: string;
	data?: UmbNotificationData;
}

export type UmbNotificationColor = '' | 'default' | 'positive' | 'warning' | 'danger';

export class UmbNotificationService {
	private _notifications: BehaviorSubject<Array<UmbNotificationHandler>> = new BehaviorSubject(
		<Array<UmbNotificationHandler>>[]
	);
	public readonly notifications: Observable<Array<UmbNotificationHandler>> = this._notifications.asObservable();

	/**
	 * @private
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @return {*}  {UmbNotificationHandler}
	 * @memberof UmbNotificationService
	 */
	private _open(options: UmbNotificationOptions<UmbNotificationData>): UmbNotificationHandler {
		const notificationHandler = new UmbNotificationHandler(options);
		notificationHandler.element.addEventListener('closed', () => this._handleClosed(notificationHandler));

		this._notifications.next([...this._notifications.getValue(), notificationHandler]);

		return notificationHandler;
	}

	/**
	 * @private
	 * @param {string} key
	 * @memberof UmbNotificationService
	 */
	private _close(key: string) {
		this._notifications.next(this._notifications.getValue().filter((notification) => notification.key !== key));
	}

	/**
	 * @private
	 * @param {string} key
	 * @memberof UmbNotificationService
	 */
	private _handleClosed(notificationHandler: UmbNotificationHandler) {
		notificationHandler.element.removeEventListener('closed', () => this._handleClosed(notificationHandler));
		this._close(notificationHandler.key);
	}

	/**
	 * Opens a notification that automatically goes away after 6 sek.
	 * @param {UmbNotificationColor} color
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @return {*}
	 * @memberof UmbNotificationService
	 */
	public peek(
		color: UmbNotificationColor,
		options: UmbNotificationOptions<UmbNotificationData>
	): UmbNotificationHandler {
		return this._open({ ...options, color });
	}

	/**
	 * Opens a notification that stays on the screen until dismissed by the user or custom code
	 * @param {UmbNotificationColor} color
	 * @param {UmbNotificationOptions<UmbNotificationData>} options
	 * @return {*}
	 * @memberof UmbNotificationService
	 */
	public stay(
		color: UmbNotificationColor,
		options: UmbNotificationOptions<UmbNotificationData>
	): UmbNotificationHandler {
		return this._open({ ...options, color, duration: null });
	}
}
