import { expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvideMixin } from './context-provide.mixin';

class MyClass {
  prop: string;

  constructor(text: string) {
    this.prop = text
  }
}

class MyTestProviderElement extends UmbContextProvideMixin(HTMLElement) {
  constructor() {
    super();
    this.provide('my-test-context-1', new MyClass('context value 1'));
    this.provide('my-test-context-2', new MyClass('context value 2'));
  }
}

customElements.define('my-test-provider-element', MyTestProviderElement);

describe('UmbContextProvideMixin', async () => {
  const element: MyTestProviderElement = await fixture(html`<my-test-provider-element></my-test-provider-element>`);
  const _providers = element['_providers'];

  it('sets all providers to element', () => {
    expect(_providers.has('my-test-context-1')).to.be.true;
    expect(_providers.has('my-test-context-2')).to.be.true;
  });

  it('can not set context with same key as already existing context', () => {
    const provider = _providers.get('my-test-context-1');
    expect(provider).to.not.be.undefined;
    if (!provider) return;
    expect(provider['_instance'].prop).to.eq('context value 1');
    element.provide('my-test-context-1', new MyClass('new context value 1'));
    expect(provider['_instance'].prop).to.eq('context value 1');
  });
});