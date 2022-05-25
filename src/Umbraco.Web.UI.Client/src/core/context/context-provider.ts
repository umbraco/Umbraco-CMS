import { umbContextRequestEventType, isUmbContextRequestEvent } from './context-request.event';
import { UmbContextProvideEventImplementation } from './context-provide.event';

/**
 * @export
 * @class UmbContextProvider
 */
export class UmbContextProvider {
  protected host: EventTarget;
  private _contextKey: string;
  private _instance: unknown;

  /**
   * Creates an instance of UmbContextProvider.
   * @param {EventTarget} host
   * @param {string} contextKey
   * @param {*} instance
   * @memberof UmbContextProvider
   */
  constructor (host: EventTarget, contextKey: string, instance: unknown) {
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
    // TODO: fire unprovided event.
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