import { UmbContextRequestEvent, UmbContextCallback } from './context-request.event';

/**
 * @export
 * @class UmbContextRequester
 */
export class UmbContextRequester {

  /**
   * Creates an instance of UmbContextRequester.
   * @param {HTMLElement} element
   * @param {string} _contextKey
   * @param {UmbContextCallback} _callback
   * @memberof UmbContextRequester
   */
  constructor (
    protected element: HTMLElement,
    private _contextKey: string,
    private _callback: UmbContextCallback
  ) {
    this.dispatchRequest();
  }

  /**
   * @memberof UmbContextRequester
   */
  dispatchRequest() {
    const event = new UmbContextRequestEvent(this._contextKey, this._callback);
    this.element.dispatchEvent(event);
  }
}