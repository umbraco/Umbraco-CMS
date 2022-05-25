import { expect } from '@open-wc/testing';
import { UmbContextRequestEventImplementation, UmbContextRequestEvent } from './context-request.event';

describe('UmbContextRequestEvent', () => {
  const contextRequestCallback = () => {
    console.log('hello from callback');
  };

  const event: UmbContextRequestEvent =  new UmbContextRequestEventImplementation('my-test-context-alias', contextRequestCallback);

  it('has context', () => {
    expect(event.contextAlias).to.eq('my-test-context-alias');
  });

  it('has a callback', () => {
    expect(event.callback).to.eq(contextRequestCallback);
  });

  it('bubbles', () => {
    expect(event.bubbles).to.be.true;
  });

  it('is composed', () => {
    expect(event.composed).to.be.true;
  });

  it('is cancelable', () => {
    expect(event.composed).to.be.true;
  });
});