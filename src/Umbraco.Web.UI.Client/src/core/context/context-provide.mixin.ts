import { UmbContextProvider } from './context-provider';

type Constructor = new (...args: any[]) => HTMLElement;

export function UmbContextProvideMixin<TBase extends Constructor>(Base: TBase) {
  return class Providing extends Base {
    _providers: Map<string, UmbContextProvider> = new Map();

    constructor(...args: any[]) {
      super();
    }

    provide(contextKey: string, instance: any) {
      if (this._providers.has(contextKey)) return;
      this._providers.set(contextKey, new UmbContextProvider(this, contextKey, instance));
      // TODO: if already connected then attach the new one.
    }

    connectedCallback() {
      super.connectedCallback?.();
      this._providers.forEach(provider => provider.attach());
    }

    disconnectedCallback() {
      super.disconnectedCallback?.();
      this._providers.forEach(provider => provider.detach());
    }
  };
}

declare global {
  interface HTMLElement {
    connectedCallback(): void;
    disconnectedCallback(): void;
  }
}