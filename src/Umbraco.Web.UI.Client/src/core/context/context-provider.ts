import { umbContextRequestEventType, isUmbContextRequestEvent } from './context-request.event';
import { UmbContextProvideEventImplementation } from './context-provide.event';

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
  constructor (host: HTMLElement, contextKey: string, instance: unknown) {
    this.host = host;
    this._contextKey = contextKey;
    this._instance = instance;
  }

  /**
   * @memberof UmbContextProvider
   */
  public attach () {
    this.host.addEventListener(umbContextRequestEventType, this._handleContextRequest);
    this.host.dispatchEvent(new UmbContextProvideEventImplementation(this._contextKey));
  }

  /**
   * @memberof UmbContextProvider
   */
  public detach () {
    this.host.removeEventListener(umbContextRequestEventType, this._handleContextRequest);
    // TODO: fire unprovide event.
  }

  /**
   * @private
   * @param {UmbContextRequestEvent} event
   * @memberof UmbContextProvider
   */
  private _handleContextRequest = (event: Event) => {
    
    if (!isUmbContextRequestEvent(event)) return;
    if (event.contextAlias !== this._contextKey) return;

    event.stopPropagation();
    event.callback(this._instance);
  }
}