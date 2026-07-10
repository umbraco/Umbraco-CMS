import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbWorkspaceActionBase } from './workspace-action-base.controller.js';

@customElement('test-workspace-action-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Concrete subclass that exposes the protected `setExecuting` for verification.
class TestWorkspaceAction extends UmbWorkspaceActionBase {
	public callSetExecuting(value: boolean) {
		this.setExecuting(value);
	}
}

describe('UmbWorkspaceActionBase', () => {
	let host: UmbTestControllerHostElement;
	const makeAction = () => new TestWorkspaceAction(host, { meta: {} as never });

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
	});

	describe('isExecuting (lazy opt-in)', () => {
		it('keeps isExecuting undefined until setExecuting() is called', () => {
			const action = makeAction();
			expect(action.isExecuting).to.be.undefined;
		});

		it('exposes the observable on first setExecuting() call', () => {
			const action = makeAction();
			action.callSetExecuting(false);
			expect(action.isExecuting).to.exist;
		});

		it('flips between true and false', async () => {
			const action = makeAction();
			action.callSetExecuting(false);
			expect(await firstValueFrom(action.isExecuting!)).to.equal(false);

			action.callSetExecuting(true);
			expect(await firstValueFrom(action.isExecuting!)).to.equal(true);

			action.callSetExecuting(false);
			expect(await firstValueFrom(action.isExecuting!)).to.equal(false);
		});

		it('emits sequential values to a single subscriber', () => {
			const action = makeAction();
			action.callSetExecuting(false);

			const emitted: boolean[] = [];
			const subscription = action.isExecuting!.subscribe((value) => emitted.push(value));

			action.callSetExecuting(true);
			action.callSetExecuting(false);

			subscription.unsubscribe();

			expect(emitted).to.deep.equal([false, true, false]);
		});

		it('reuses the same observable across multiple setExecuting() calls', () => {
			const action = makeAction();
			action.callSetExecuting(false);
			const firstRef = action.isExecuting;
			action.callSetExecuting(true);
			expect(action.isExecuting).to.equal(firstRef);
		});
	});
});
