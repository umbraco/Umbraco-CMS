import { LogLevelModel, LogMessagePropertyModel } from '@umbraco-cms/backend-api';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';

@customElement('umb-log-viewer-message')
export class UmbLogViewerMessageElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			summary {
				display: flex;
			}

			summary > div {
				padding: 10px 20px;
				display: flex;
				align-items: center;
			}

			#timestamp {
				flex: 2;
			}

			#level,
			#machine {
				flex: 1;
			}

			#message {
				flex: 4;
			}
		`,
	];

	@property()
	timestamp = '';

	@state()
	date?: Date;

	@property()
	level: LogLevelModel | '' = '';

	@property()
	messageTemplate = '';

	@property()
	renderedMessage = '';

	@property({ attribute: false })
	properties: Array<LogMessagePropertyModel> = [];

	@property()
	exception = '';

	willUpdate(changedProperties: Map<string | number | symbol, unknown>) {
		if (changedProperties.has('timestamp')) {
			this.date = new Date(this.timestamp);
		}
	}

	render() {
		return html`
			<details>
				<summary>
					<div id="timestamp">${this.date?.toLocaleString()}</div>
					<div id="level">${this.level}</div>
					<div id="machine">${this.properties.find((property) => property.name === 'MachineName')?.value}</div>
					<div id="message">${this.renderedMessage}</div>
				</summary>
				<ul>
					${this.properties.map((property) => html`<li>${property.name}: ${property.value}</li>`)}
				</ul>
			</details>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-log-viewer-message': UmbLogViewerMessageElement;
	}
}
