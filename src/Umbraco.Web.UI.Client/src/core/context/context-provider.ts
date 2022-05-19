import { umbContextRequestType, isUmbContextRequestEvent } from './context-request.event';
import { UmbContextProvideEvent } from './context-provide.event';

/**
 * @export
 * @class UmbContextProvider
 */
export class UmbContextProvider {
  protected host: HTMLElement;
  private _contextKey: string;
  private _instance: any;

  /**
   * Creates an instance of UmbContextProvider.
   * @param {HTMLElement} host
   * @param {string} contextKey
   * @param {*} instance
   * @memberof UmbContextProvider
   */
  constructor (host: HTMLElement, contextKey: string, instance: any) {
    this.host = host;
    this._contextKey = contextKey;
    this._instance = instance;
    this.attach();
  }

  /**
   * @memberof UmbContextProvider
   */
  public attach () {
    this.host.addEventListener(umbContextRequestType, this._handleContextRequest);
    this.host.dispatchEvent(new UmbContextProvideEvent(this._contextKey));
  }

  /**
   * @memberof UmbContextProvider
   */
  public detach () {
    this.host.removeEventListener(umbContextRequestType, this._handleContextRequest);
  }

  /**
   * @private
   * @param {UmbContextRequestEvent} event
   * @memberof UmbContextProvider
   */
  private _handleContextRequest = (event: Event) => {
    if (!isUmbContextRequestEvent(event)) return;
    if (event.contextKey !== this._contextKey) return;

    event.stopPropagation();
    event.callback(this._instance);
  }
}