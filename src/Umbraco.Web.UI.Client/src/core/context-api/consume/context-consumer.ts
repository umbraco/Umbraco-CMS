import { isUmbContextProvideEventType, umbContextProvideEventType } from '../provide/context-provide.event';
import { UmbContextRequestEventImplementation, UmbContextCallback } from './context-request.event';

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
	constructor(protected target: EventTarget, private _contextAlias: string, private _callback: UmbContextCallback) {}

	/**
	 * @memberof UmbContextConsumer
	 */
	public request() {
		const event = new UmbContextRequestEventImplementation(this._contextAlias, this._callback);
		this.target.dispatchEvent(event);
	}

	public hostConnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.addEventListener(umbContextProvideEventType, this._handleNewProvider);
		this.request();
	}

	public hostDisconnected() {
		// TODO: We need to use closets application element. We need this in order to have separate Backoffice running within or next to each other.
		window.removeEventListener(umbContextProvideEventType, this._handleNewProvider);
	}

	private _handleNewProvider = (event: Event) => {
		if (!isUmbContextProvideEventType(event)) return;

		if (this._contextAlias === event.contextAlias) {
			this.request();
		}
	};
}
