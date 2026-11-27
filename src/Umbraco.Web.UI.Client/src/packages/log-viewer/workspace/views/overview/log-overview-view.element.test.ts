import type { UmbLogViewerOverviewViewElement } from './log-overview-view.element.js';
import { UmbLogViewerWorkspaceElement } from '../../logviewer-workspace.element.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbArrayState, UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

import '../../../../../libs/context-api/provide/context-provider.element.js';
import './components/index.js';
import './log-overview-view.element.js';

const context = {
	logCount: new UmbObjectState({
		Verbose: 0,
		Debug: 0,
		Information: 20,
		Warning: 3,
		Error: 18,
		Fatal: 0,
	}).asObservable(),
	canShowLogs: new UmbBooleanState(false).asObservable(),
	loggers: new UmbArrayState([{ name: 'Global', level: 'Information' }], (logger) => logger.name).asObservable(),
	dateRange: new UmbObjectState({ startDate: '2026-01-01', endDate: '2026-01-02' }).asObservable(),
	today: '2026-01-02',
	getLogLevels: () => undefined,
	getLogCount: () => undefined,
	getSavedSearches: () => undefined,
	setDateRange: () => undefined,
	getHostElement: () => document.body,
};

@customElement('umb-test-log-viewer-overview-host')
class UmbTestLogViewerOverviewHostElement extends LitElement {
	static override styles = UmbLogViewerWorkspaceElement.styles;

	override render() {
		return html`<umb-context-provider key="UmbLogViewerWorkspaceContext" .value=${context}>
			<umb-log-viewer-overview-view></umb-log-viewer-overview-view>
		</umb-context-provider>`;
	}
}

describe('UmbLogViewerOverviewViewElement', () => {
	let overview: UmbLogViewerOverviewViewElement;

	beforeEach(async () => {
		const host = await fixture<UmbTestLogViewerOverviewHostElement>(
			html`<umb-test-log-viewer-overview-host></umb-test-log-viewer-overview-host>`,
		);
		overview = host.shadowRoot!.querySelector('umb-log-viewer-overview-view')!;
		await aTimeout(0);
		await overview.updateComplete;
	});

	it('lays out in a single column when it is narrow', async () => {
		overview.style.width = '280px';
		await aTimeout(0);

		const layout = overview.shadowRoot!.querySelector<HTMLElement>('#logviewer-layout')!;
		const info = overview.shadowRoot!.querySelector<HTMLElement>('#info')!;
		expect(getComputedStyle(layout).gridTemplateColumns.split(' ')).to.have.lengthOf(1);
		expect(info.getBoundingClientRect().width).to.be.closeTo(layout.getBoundingClientRect().width, 1);
	});
});
