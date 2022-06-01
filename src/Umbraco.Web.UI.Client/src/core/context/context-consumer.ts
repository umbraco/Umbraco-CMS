import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event';
import { isUmbContextProvideEvent, umbContextProvideEventType } from './context-provide.event';

/**
 * @export
 * @class UmbContextConsumer
 */
export class UmbContextConsumer {

  /**
   * Creates an instance of UmbContextConsumer.
   * @param {EventTarget} target
   * @param {string} _contextAlias
   * @param {UmbContextCallback} _callback
   * @memberof UmbContextConsumer
   */
  constructor (
    protected target: EventTarget,
    private _contextAlias: string,
    private _callback: UmbContextCallback
  ) {
    
  }

  /**
   * @memberof UmbContextConsumer
   */
   public request() {
    const event = new UmbContextRequestEventImplementation(this._contextAlias, this._callback);
    this.target.dispatchEvent(event);
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

    if (this._contextAlias === event.contextAlias) {
      this.request();
    }
  }
}