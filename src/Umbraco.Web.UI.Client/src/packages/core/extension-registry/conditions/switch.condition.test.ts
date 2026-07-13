import { UmbSwitchCondition, type SwitchConditionConfig } from './switch.condition.js';
import { aTimeout, expect, fixture } from '@open-wc/testing';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-switch-condition-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestSwitchConditionHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const baseConfig = (frequency: string): SwitchConditionConfig => ({
	alias: 'Umb.Condition.Switch',
	frequency,
});

describe('UmbSwitchCondition', () => {
	let host: UmbControllerHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-switch-condition-host></umb-test-switch-condition-host>`);
	});

	it('starts not permitted', () => {
		const condition = new UmbSwitchCondition(host, {
			config: baseConfig('30'),
			onChange: () => {},
		});
		expect(condition.permitted).to.be.false;
		condition.destroy();
	});

	it('flips between permitted=true and permitted=false at the configured frequency', async () => {
		// Wait for two transitions (false→true→false) and assert the sequence.
		// We listen to onChange instead of polling `permitted` at fixed times,
		// since cumulative wait drift would otherwise make assertions racy.
		const frequencyMs = 30;
		const transitions: Array<boolean> = [];
		let timeout: ReturnType<typeof setTimeout> | undefined;
		let condition: UmbSwitchCondition | undefined;

		try {
			await new Promise<void>((resolve, reject) => {
				timeout = setTimeout(() => {
					condition?.destroy();
					reject(new Error('Switch condition did not flip twice in time'));
				}, 1000);

				condition = new UmbSwitchCondition(host, {
					config: baseConfig(String(frequencyMs)),
					onChange: (permitted) => {
						transitions.push(permitted);
						if (transitions.length === 2) {
							clearTimeout(timeout);
							condition?.destroy();
							resolve();
						}
					},
				});
			});
		} finally {
			if (timeout) clearTimeout(timeout);
			condition?.destroy();
		}

		expect(transitions).to.eql([true, false]);
	});

	it('does not flip after destroy()', async () => {
		const frequencyMs = 30;
		let callCount = 0;

		const condition = new UmbSwitchCondition(host, {
			config: baseConfig(String(frequencyMs)),
			onChange: () => callCount++,
		});

		// Destroy before the first transition fires.
		condition.destroy();
		await aTimeout(frequencyMs * 3 + 50);
		expect(callCount).to.equal(0);
	});

	it('throws when frequency is not a positive number', () => {
		expect(
			() => new UmbSwitchCondition(host, { config: baseConfig('0'), onChange: () => {} }),
		).to.throw(/Frequency must be a positive number/);

		expect(
			() => new UmbSwitchCondition(host, { config: baseConfig('-5'), onChange: () => {} }),
		).to.throw(/Frequency must be a positive number/);

		expect(
			() => new UmbSwitchCondition(host, { config: baseConfig('NaN-string'), onChange: () => {} }),
		).to.throw(/Frequency must be a positive number/);
	});
});
