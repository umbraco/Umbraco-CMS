import { css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-log-viewer-too-many-logs-warning')
export class UmbLogViewerTooManyLogsWarningElement extends UmbLitElement {
	override render() {
		return html`<uui-box id="to-many-logs-warning">
			<h3><umb-localize key="logViewer_unableToViewLogs">Unable to view logs</umb-localize></h3>
			<p>
				<umb-localize key="logViewer_logFileTooLarge"
					>Today's log file is too large to be viewed and would cause performance problems.</umb-localize
				>
			</p>
			<p>
				<umb-localize key="logViewer_narrowDateRangeOrOpenManually"
					>If you need to view the log files, narrow your date range or try opening them manually.</umb-localize
				>
			</p>
		</uui-box>`;
	}

	static override styles = [
		css`
			:host {
				text-align: center;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-too-many-logs-warning': UmbLogViewerTooManyLogsWarningElement;
	}
}
