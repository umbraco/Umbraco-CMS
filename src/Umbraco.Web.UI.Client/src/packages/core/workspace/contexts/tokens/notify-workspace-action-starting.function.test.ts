import { expect } from '@open-wc/testing';
import { notifyWorkspaceActionStarting } from './notify-workspace-action-starting.function.js';

describe('notifyWorkspaceActionStarting', () => {
	it('is a no-op when options are undefined', () => {
		expect(() => notifyWorkspaceActionStarting()).to.not.throw();
	});

	it('is a no-op when the callback is undefined', () => {
		expect(() => notifyWorkspaceActionStarting({})).to.not.throw();
	});

	it('invokes onActionStarting when provided', () => {
		let called = 0;
		notifyWorkspaceActionStarting({ onActionStarting: () => called++ });
		expect(called).to.equal(1);
	});
});
