import type { UmbNotificationOptions, UmbNotificationColor, UmbNotificationDefaultData } from './types.js';
import type { UUIToastNotificationElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';

const DEFAULT_LAYOUT = 'umb-notification-layout-default';

/**
 * @class UmbNotificationHandler
 */
export class UmbNotificationHandler {
	private _closeResolver: any;
	private _closePromise: Promise<any>;
	private _elementName?: string;
	private _data?: UmbNotificationDefaultData;

	private _defaultColor: UmbNotificationColor = 'default';
	private _defaultDuration = 6000;

	public key: string;
	public element!: UUIToastNotificationElement;
	public color: UmbNotificationColor;
	public duration: number | null;

	/**
	 * Creates an instance of UmbNotificationHandler.
	 * @param {UmbNotificationOptions} options
	 * @memberof UmbNotificationHandler
	 */
	constructor(options: UmbNotificationOptions) {
		this.key = UmbId.new();
		this.color = options.color || this._defaultColor;
		this.duration = options.duration !== undefined ? options.duration : this._defaultDuration;

		this._elementName = options.elementName || DEFAULT_LAYOUT;
		this._data = options.data;

		this._closePromise = new Promise((res) => {
			this._closeResolver = res;
		});

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
	 * @returns {*}
	 * @memberof UmbNotificationHandler
	 */
	public onClose(): Promise<any> {
		return this._closePromise;
	}
}
