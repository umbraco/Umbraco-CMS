import { v4 as uuidv4 } from 'uuid';
import { UmbNotificationOptions, UmbNotificationData, UmbNotificationColor } from './notification.service';

/**
 * @export
 * @class UmbNotificationHandler
 */
export class UmbNotificationHandler {
  private _closeResolver: any;
  private _closePromise: Promise<any>;
  private _elementName?: string;
  private _data: UmbNotificationData;

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
  constructor (options: UmbNotificationOptions<UmbNotificationData>) {
    this.key = uuidv4();
    this.color = options.color || this._defaultColor;
    this.duration = options.duration !== undefined ? options.duration : this._defaultDuration;

    this._elementName = options.elementName || this._defaultLayout;
    this._data = options.data;

    this._closePromise = new Promise(res => {
      this._closeResolver = res;
    });

    this._createLayoutElement();
  }

  /**
   * @private
   * @memberof UmbNotificationHandler
   */
  private _createLayoutElement () {
    if (!this._elementName) return;
    this.element = document.createElement(this._elementName);
    this.element.data = this._data;
    this.element.notificationHandler = this;
  }
  
  /**
   * @param {...any} args
   * @memberof UmbNotificationHandler
   */
  public close (...args: any) {
    this._closeResolver(...args);
  }

  /**
   * @return {*} 
   * @memberof UmbNotificationHandler
   */
  public onClose (): Promise<any> {
    return this._closePromise;
  }
}