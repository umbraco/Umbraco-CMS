import { expect } from '@open-wc/testing';
import { UmbContextRequestEvent } from './context-request.event';

describe('UmbContextRequestEvent', () => {
  const contextRequestCallback = () => {
    console.log('hello from callback');
  };

  const contextRequestEvent: UmbContextRequestEvent =  new UmbContextRequestEvent('my-test-context', contextRequestCallback);

  it('has context', () => {
    expect(contextRequestEvent.contextKey).to.eq('my-test-context');
  });

  it('has a callback', () => {
    expect(contextRequestEvent.callback).to.eq(contextRequestCallback);
  });

  it('bubbles', () => {
    expect(contextRequestEvent.bubbles).to.be.true;
  });

  it('is composed', () => {
    expect(contextRequestEvent.composed).to.be.true;
  });

  it('is cancelable', () => {
    expect(contextRequestEvent.composed).to.be.true;
  });
});