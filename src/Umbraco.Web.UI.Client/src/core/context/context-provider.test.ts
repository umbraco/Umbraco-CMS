import { expect } from '@open-wc/testing';
import { UmbContextProvider } from './context-provider';
import { UmbContextRequester } from './context-requester';
import { UmbContextRequestEvent } from './context-request.event';

class MyClass {
  prop = 'value from provider';
}

describe('UmbContextProvider', () => {
  let provider: UmbContextProvider;

  beforeEach(() => {
    provider = new UmbContextProvider(document.body, 'my-test-context', new MyClass());
  });

  afterEach(async () => {
    provider.detach();
  });

  describe('Public API', () => {

    describe('properties', () => {
      it('has a host property', () => {
        expect(provider).to.have.property('host');
      });
    });

    describe('methods', () => {
      it('has an attach method', () => {
        expect(provider).to.have.property('attach').that.is.a('function');
      });

      it('has a detach method', () => {
        expect(provider).to.have.property('detach').that.is.a('function');
      });
    });
  });

  it('handles context request events', (done) => {
    const event = new UmbContextRequestEvent('my-test-context', (_instance) => {
      expect(_instance.prop).to.eq('value from provider');
      done();
    });

    document.body.dispatchEvent(event);
  });

  it('works with UmbContextRequester', (done) => {
    const element = document.createElement('div');
    document.body.appendChild(element);

    new UmbContextRequester(element, 'my-test-context', (_instance) => {
      expect(_instance.prop).to.eq('value from provider');
      done();
    });
  });
});