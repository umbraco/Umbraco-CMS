import { UmbContextRequester } from './context-requester';
import { umbContextProvideType, isUmbContextProvideEvent } from './context-provide.event';

type Constructor = new (...args: any[]) => HTMLElement;

export function UmbContextInjectMixin<TBase extends Constructor>(Base: TBase) {
  return class Providing extends Base {
    // all context requesters in the element
    _requesters: Map<string, UmbContextRequester> = new Map();
     // all successfully resolved context requests
    _resolved: Map<string, any> = new Map();

    /**
     * Requests a context from any parent provider
     * @param {string} contextKey
     */
    requestContext(contextKey: string) {
      if (this._requesters.has(contextKey)) {
        return;
      }

      const requester = new UmbContextRequester(this, contextKey, (_instance: any) => {
        this._resolved.set(contextKey, _instance);
        this.contextInjected(this._resolved);
      });

      this._requesters.set(contextKey, requester);
    }

    /**
     * Sends a new context request for when a new provider is added.
     * It only sends requests that matches the provider context key.
     * @private
     * @param {UmbContextProvideEvent} event
     */
    handleNewProvider = (event: Event) => {
      if (!isUmbContextProvideEvent(event)) return;

      if (this._requesters.has(event.contextKey)) {
        const requester = this._requesters.get(event.contextKey);
        requester?.dispatchRequest();
      }
    }

    connectedCallback() {
      super.connectedCallback?.();
      this._requesters.forEach(requester => requester.dispatchRequest());
      window.addEventListener(umbContextProvideType, this.handleNewProvider);
    }

    disconnectedCallback() {
      super.disconnectedCallback?.();
      window.removeEventListener(umbContextProvideType, this.handleNewProvider);
    }

    /**
     * This is called once a context was successfully requested.
     * Run logic here when the dependecy is ready.
     * @param contexts Map of all resolved contexts
     */
    contextInjected(contexts: Map<string, any>) {
      // This is a stub
    }
  };
}

declare global {
  interface HTMLElement {
    connectedCallback(): void;
    disconnectedCallback(): void;
  }
}