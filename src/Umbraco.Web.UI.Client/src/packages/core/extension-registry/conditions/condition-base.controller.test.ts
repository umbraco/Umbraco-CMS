import { UmbConditionBase } from './condition-base.controller.js';
import { expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-test-condition-base-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestConditionBaseHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestCondition extends UmbConditionBase<UmbConditionConfigBase> {}

describe('UmbConditionBase', () => {
	let host: UmbControllerHostElement;
	const config: UmbConditionConfigBase = { alias: 'Umb.Test.Condition' };

	beforeEach(async () => {
		host = await fixture(html`<umb-test-condition-base-host></umb-test-condition-base-host>`);
	});

	it('exposes the config it was constructed with', () => {
		const condition = new UmbTestCondition(host, { config, onChange: () => {} });
		expect(condition.config).to.equal(config);
	});

	it('initializes with permitted=false', () => {
		const condition = new UmbTestCondition(host, { config, onChange: () => {} });
		expect(condition.permitted).to.be.false;
	});

	it('invokes onChange when permitted transitions to a new value', () => {
		let calls: Array<boolean> = [];
		const condition = new UmbTestCondition(host, {
			config,
			onChange: (permitted) => calls.push(permitted),
		});

		condition.permitted = true;
		expect(calls).to.eql([true]);

		condition.permitted = false;
		expect(calls).to.eql([true, false]);
	});

	it('does NOT invoke onChange when permitted is set to the current value', () => {
		let callCount = 0;
		const condition = new UmbTestCondition(host, {
			config,
			onChange: () => callCount++,
		});

		// Same as initial value (false) — should not fire.
		condition.permitted = false;
		expect(callCount).to.equal(0);

		// Real transition — should fire once.
		condition.permitted = true;
		expect(callCount).to.equal(1);

		// Same value again — should not fire.
		condition.permitted = true;
		expect(callCount).to.equal(1);
	});

	it('does not invoke onChange after destroy()', () => {
		let callCount = 0;
		const condition = new UmbTestCondition(host, {
			config,
			onChange: () => callCount++,
		});

		condition.destroy();
		// After destroy the internal onChange reference is cleared, so further
		// permitted writes must not trigger any callback.
		condition.permitted = true;
		expect(callCount).to.equal(0);
	});

	it('clears its config reference on destroy', () => {
		const condition = new UmbTestCondition(host, { config, onChange: () => {} });
		condition.destroy();
		expect(condition.config).to.be.undefined;
	});
});
