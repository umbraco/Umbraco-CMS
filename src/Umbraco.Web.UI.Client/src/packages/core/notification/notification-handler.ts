import type { UmbNotificationOptions, UmbNotificationColor, UmbNotificationDefaultData } from './types.js';
import type { UUIToastNotificationElement } from '@umbraco-cms/backoffice/external/uui';
import { UmbId } from '@umbraco-cms/backoffice/id';

const DEFAULT_LAYOUT = 'umb-notification-layout-default';

/**
 * @class UmbNotificationHandler
 */
export class UmbNotificationHandler<UmbNotificationData = UmbNotificationDefaultData> {
	private _closeResolver: any;
	private _closePromise: Promise<any>;
	private _elementName?: string;
	private _data?: UmbNotificationData;
	private _layoutElement: HTMLElement & { data?: UmbNotificationData };

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
	constructor(options: UmbNotificationOptions<UmbNotificationData>) {
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

		this._layoutElement = document.createElement(this._elementName) as HTMLElement & { data?: UmbNotificationData };
		this._layoutElement.data = this._data;
		(this._layoutElement as any).notificationHandler = this;

		notification.appendChild(this._layoutElement);

		this.element = notification;
	}

	/**
	 * Updates the notification data and refreshes the layout element.
	 * @param {UmbNotificationData} data - The new data to display
	 * @memberof UmbNotificationHandler
	 */
	public updateData(data: UmbNotificationData): void {
		this._data = data;
		this._layoutElement.data = data;
	}

	/**
	 * Updates the notification color.
	 * @param {UmbNotificationColor} color - The new color for the notification
	 * @memberof UmbNotificationHandler
	 */
	public updateColor(color: UmbNotificationColor): void {
		this.color = color;
		this.element.color = color;
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
