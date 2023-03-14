import { UUIToastNotificationElement } from '@umbraco-ui/uui';
import { v4 as uuidv4 } from 'uuid';
import type { UmbNotificationOptions, UmbNotificationColor, UmbNotificationDefaultData } from './notification.context';

/**
 * @export
 * @class UmbNotificationHandler
 */
export class UmbNotificationHandler {
	private _closeResolver: any;
	private _closePromise: Promise<any>;
	private _elementName?: string;
	private _data?: UmbNotificationDefaultData;

	private _defaultColor: UmbNotificationColor = 'default';
	private _defaultDuration = 6000;
	private _defaultLayout = 'umb-notification-layout-default';

	public key: string;
	public element: any;
	public color: UmbNotificationColor;
	public duration: number | null;

	/**
	 * Creates an instance of UmbNotificationHandler.
	 * @param {UmbNotificationOptions} options
	 * @memberof UmbNotificationHandler
	 */
	constructor(options: UmbNotificationOptions) {
		this.key = uuidv4();
		this.color = options.color || this._defaultColor;
		this.duration = options.duration !== undefined ? options.duration : this._defaultDuration;

		this._elementName = options.elementName || this._defaultLayout;
		this._data = options.data;

		this._closePromise = new Promise((res) => {
			this._closeResolver = res;
		});

		this._createElement();
	}

	/**
	 * @private
	 * @memberof UmbNotificationHandler
	 */
	private _createElement() {
		if (!this._elementName) return;

		const notification: UUIToastNotificationElement = document.createElement('uui-toast-notification');

		notification.color = this.color;
		notification.autoClose = this.duration;

		const element: any = document.createElement(this._elementName);
		element.data = this._data;
		element.notificationHandler = this;

		notification.appendChild(element);

		this.element = notification;
	}

	/**
	 * @param {...any} args
	 * @memberof UmbNotificationHandler
	 */
	public close(...args: any) {
		this._closeResolver(...args);
		this.element.open = false;
	}

	/**
	 * @return {*}
	 * @memberof UmbNotificationHandler
	 */
	public onClose(): Promise<any> {
		return this._closePromise;
	}
}
