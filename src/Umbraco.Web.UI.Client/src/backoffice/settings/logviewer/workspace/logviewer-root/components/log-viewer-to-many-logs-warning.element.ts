import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-log-viewer-to-many-logs-warning')
export class UmbLogViewerToManyLogsWarningElement extends LitElement {
	static styles = [
		css`
			:host {
				text-align: center;
			}
		`,
	];

	render() {
		return html`<uui-box id="to-many-logs-warning">
			<h3>Unable to view logs</h3>
			<p>Today's log file is too large to be viewed and would cause performance problems.</p>
			<p>If you need to view the log files, narrow your date range or try opening them manually.</p>
		</uui-box>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-to-many-logs-warning': UmbLogViewerToManyLogsWarningElement;
	}
}
