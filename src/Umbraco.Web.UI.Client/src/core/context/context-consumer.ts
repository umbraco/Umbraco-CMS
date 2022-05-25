import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event';
import { isUmbContextProvideEvent, umbContextProvideEventType } from './context-provide.event';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer {

  /**
   * Creates an instance of UmbContextConsumer.
   * @param {HTMLElement} element
   * @param {string} _contextKey
   * @param {UmbContextCallback} _callback
   * @memberof UmbContextConsumer
   */
  constructor (
    protected element: HTMLElement,
    private _contextKey: string,
    private _callback: UmbContextCallback
  ) {
    
  }

  /**
   * @memberof UmbContextConsumer
   */
   public request() {
    const event = new UmbContextRequestEventImplementation(this._contextKey, this._callback);
    this.element.dispatchEvent(event);
  }

  public attach() {
    window.addEventListener(umbContextProvideEventType, this._handleNewProvider);
    this.request();
  }

  public detach() {
    window.removeEventListener(umbContextProvideEventType, this._handleNewProvider);
  } 

  private _handleNewProvider = (event: Event) => {
    if (!isUmbContextProvideEvent(event)) return;

    if (this._contextKey === event.contextAlias) {
      this.request();
    }
  }
}