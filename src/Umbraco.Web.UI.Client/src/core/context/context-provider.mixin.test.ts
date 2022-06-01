import { expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvider } from './context-provider';
import { UmbContextProviderMixin } from './context-provider.mixin';

class MyClass {
  prop: string;

  constructor(text: string) {
    this.prop = text;
  }
}

class MyTestProviderElement extends UmbContextProviderMixin(HTMLElement) {
  constructor() {
    super();
    this.provideContext('my-test-context-1', new MyClass('context value 1'));
    this.provideContext('my-test-context-2', new MyClass('context value 2'));
  }
}

customElements.define('my-test-provider-element', MyTestProviderElement);

describe('UmbContextProviderMixin', async () => {
  let element: MyTestProviderElement;
  let _providers: Map<string, UmbContextProvider>;

  beforeEach(async () => {
    element = await fixture(html`<my-test-provider-element></my-test-provider-element>`);
    _providers = (element as any)['_providers'];
  });

  it('sets all providers to element', () => {
    expect(_providers.has('my-test-context-1')).to.be.true;
    expect(_providers.has('my-test-context-2')).to.be.true;
  });

  it('can not set context with same key as already existing context', () => {
    const provider = _providers.get('my-test-context-1');
    expect(provider).to.not.be.undefined;
    if (!provider) return;
    expect((provider['_instance'] as MyClass).prop).to.eq('context value 1');
    element.provideContext('my-test-context-1', new MyClass('new context value 1'));
    expect((provider['_instance'] as MyClass).prop).to.eq('context value 1');
  });
});
