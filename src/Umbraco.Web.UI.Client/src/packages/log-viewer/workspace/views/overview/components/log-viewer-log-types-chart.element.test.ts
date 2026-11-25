import type { UmbLogViewerLogTypesChartElement } from './log-viewer-log-types-chart.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

import '../../../../../../libs/context-api/provide/context-provider.element.js';
import '../../../../components/donut-chart/index.js';
import './log-viewer-log-types-chart.element.js';

const counts = { Verbose: 0, Debug: 50, Information: 900, Warning: 40, Error: 10, Fatal: 0 };

function legend(element: UmbLogViewerLogTypesChartElement) {
	return [...element.shadowRoot!.querySelectorAll('#legend button')].map((button) => ({
		level: button.children[1].textContent!.trim(),
		share: button.querySelector('.share')!.textContent!.trim(),
	}));
}

function chartShares(element: UmbLogViewerLogTypesChartElement) {
	const chart = element.shadowRoot!.querySelector('umb-donut-chart')!;
	return [...chart.shadowRoot!.querySelectorAll('desc')].map((desc) => {
		const [, level, share] = desc.textContent!.match(/^(\w+):.*\((.+)\)$/)!;
		return { level, share };
	});
}

async function settle(element: UmbLogViewerLogTypesChartElement) {
	for (let i = 0; i < 3; i++) {
		await aTimeout(0);
		await element.updateComplete;
		await element.shadowRoot!.querySelector('umb-donut-chart')?.updateComplete;
	}
}

describe('UmbLogViewerLogTypesChartElement', () => {
	let element: UmbLogViewerLogTypesChartElement;

	beforeEach(async () => {
		const context = {
			logCount: new UmbObjectState(counts).asObservable(),
			dateRange: new UmbObjectState({ startDate: '', endDate: '' }).asObservable(),
			getLogCount: () => undefined,
			getHostElement: () => document.body,
		};
		const provider = await fixture(html`
			<umb-context-provider key="UmbLogViewerWorkspaceContext" .value=${context}>
				<umb-log-viewer-log-types-chart></umb-log-viewer-log-types-chart>
			</umb-context-provider>
		`);
		element = provider.querySelector('umb-log-viewer-log-types-chart')!;
		await settle(element);
	});

	it('shows the same share for each level in the legend as the chart does', async () => {
		expect(legend(element)).to.deep.equal([
			{ level: 'Debug', share: '5%' },
			{ level: 'Information', share: '90%' },
			{ level: 'Warning', share: '4%' },
			{ level: 'Error', share: '1%' },
		]);
		expect(chartShares(element)).to.deep.equal(legend(element));
	});

	it('shows the share of the levels still shown, and no share for a level that is filtered out', async () => {
		element.shadowRoot!.querySelectorAll<HTMLButtonElement>('#legend button')[1].click();
		await settle(element);

		expect(legend(element)).to.deep.equal([
			{ level: 'Debug', share: '50%' },
			{ level: 'Information', share: '' },
			{ level: 'Warning', share: '40%' },
			{ level: 'Error', share: '10%' },
		]);
		expect(chartShares(element)).to.deep.equal(legend(element).filter(({ share }) => share));
	});
});
