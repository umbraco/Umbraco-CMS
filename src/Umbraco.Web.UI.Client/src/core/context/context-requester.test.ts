import { expect, oneEvent } from '@open-wc/testing';
import { UmbContextProvider } from './context-provider';
import { UmbContextRequester } from './context-requester';
import { UmbContextRequestEvent, umbContextRequestType } from './context-request.event';

const testContextKey = 'my-test-context';

class MyClass {
  prop = 'value from provider';
}

describe('UmbContextRequester', () => {
  let requestor: UmbContextRequester;

  beforeEach(() => {
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    requestor = new UmbContextRequester(document.body, testContextKey, () => {});
  });

  describe('Public API', () => {
    describe('methods', () => {
      it('has a dispatchRequest method', () => {
        expect(requestor).to.have.property('dispatchRequest').that.is.a('function');
      });
    });

    describe('events', () => {
      it('dispatches request context event when constructed', async () => {
        const listener = oneEvent(window, umbContextRequestType);
        // eslint-disable-next-line @typescript-eslint/no-empty-function
        new UmbContextRequester(document.body, testContextKey, () => {});
        const event = await listener as unknown as UmbContextRequestEvent;
        expect(event).to.exist;
        expect(event.type).to.eq(umbContextRequestType);
        expect(event.contextKey).to.eq(testContextKey);
      });
    });
  });

  it('works with UmbContextProvider', (done: any) => {
    const provider = new UmbContextProvider(document.body, testContextKey, new MyClass());

    const element = document.createElement('div');
    document.body.appendChild(element);

    new UmbContextRequester(element, testContextKey, (_instance) => {
      expect(_instance.prop).to.eq('value from provider');
      done();
    });

    provider.detach();
  });

});