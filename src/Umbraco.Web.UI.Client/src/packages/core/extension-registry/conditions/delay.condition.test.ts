import { UmbDelayCondition, type DelayConditionConfig } from './delay.condition.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-delay-condition-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestDelayConditionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const baseConfig = (offset: string): DelayConditionConfig => ({
	alias: 'Umb.Condition.Delay',
	offset,
});

describe('UmbDelayCondition', () => {
	let host: UmbControllerHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-delay-condition-host></umb-test-delay-condition-host>`);
	});

	it('starts not permitted', () => {
		const condition = new UmbDelayCondition(host, {
			host,
			config: baseConfig('30'),
			onChange: () => {},
		});
		expect(condition.permitted).to.be.false;
		condition.destroy();
	});

	it('becomes permitted after the configured offset has elapsed', async () => {
		const offsetMs = 30;
		const transitions: Array<boolean> = [];

		const condition = new UmbDelayCondition(host, {
			host,
			config: baseConfig(String(offsetMs)),
			onChange: (permitted) => transitions.push(permitted),
		});

		// Not yet — well before the timer fires.
		await aTimeout(5);
		expect(condition.permitted).to.be.false;
		expect(transitions).to.eql([]);

		// Wait long enough for the timer (with a generous margin to keep the test stable).
		await aTimeout(offsetMs + 50);
		expect(condition.permitted).to.be.true;
		expect(transitions).to.eql([true]);

		condition.destroy();
	});

	it('does not fire onChange after destroy() when destroyed before the timer elapses', async () => {
		let callCount = 0;
		const condition = new UmbDelayCondition(host, {
			host,
			config: baseConfig('30'),
			onChange: () => callCount++,
		});

		condition.destroy();
		// Wait past the original offset — no callback should run.
		await aTimeout(60);
		expect(callCount).to.equal(0);
	});

	it('throws when offset is not a positive number', () => {
		expect(
			() => new UmbDelayCondition(host, { host, config: baseConfig('0'), onChange: () => {} }),
		).to.throw(/Offset must be a positive number/);

		expect(
			() => new UmbDelayCondition(host, { host, config: baseConfig('-5'), onChange: () => {} }),
		).to.throw(/Offset must be a positive number/);

		expect(
			() => new UmbDelayCondition(host, { host, config: baseConfig('not-a-number'), onChange: () => {} }),
		).to.throw(/Offset must be a positive number/);
	});
});
