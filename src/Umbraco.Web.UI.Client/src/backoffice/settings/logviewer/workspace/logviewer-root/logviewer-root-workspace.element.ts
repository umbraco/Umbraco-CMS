import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLogViewerWorkspaceContext } from './logviewer-root.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { SavedLogSearch } from '@umbraco-cms/backend-api';

@customElement('umb-logviewer-root-workspace')
export class UmbLogViewerRootWorkspaceElement extends UmbLitElement {
	static styles = [
		css`
			:host {
				display: block;
			}

			#header {
				display: flex;
				padding: 0 var(--uui-size-space-6);
				gap: var(--uui-size-space-4);
				width: 100%;
			}

			#logviewer-layout {
				margin: 20px;
			}

			#logviewer-layout {
				height: calc(100vh - 215px);
				display: grid;
				grid-template-columns: 4fr 1fr;
				grid-template-rows: 1fr 1fr;
				gap: 20px 20px;
				grid-auto-flow: row;
				grid-template-areas:
					'saved-searches info'
					'common-messages info';
			}

			#info {
				grid-area: info;
				align-self: start;
				display: grid;
				grid-template-columns: repeat(2, 1fr);
				grid-template-rows: repeat(4, 1fr);
				gap: 20px 20px;
			}

			#time-period {
				grid-area: 1 / 1 / 2 / 3;
			}

			#errors {
				grid-area: 2 / 1 / 3 / 2;
			}

			#level {
				grid-area: 2 / 2 / 3 / 3;
			}

			#types {
				grid-area: 3 / 1 / 5 / 3;
			}

			#saved-searches-container {
				grid-area: saved-searches;
			}

			#common-messages-container {
				grid-area: common-messages;
			}

			#saved-searches-container > uui-box,
			#common-messages-container > uui-box {
				height: 100%;
			}
		`,
	];

	@state()
	private _savedSearches: SavedLogSearch[] = [];

	load(): void {
		// Not relevant for this workspace -added to prevent the error from popping up
		console.log('Loading something from somewhere');
	}

	create(): void {
		// Not relevant for this workspace
	}

	#logViewerContext = new UmbLogViewerWorkspaceContext(this);

	async connectedCallback() {
		super.connectedCallback();

		this.observe(this.#logViewerContext.savedSearches, (savedSearches) => {
			this._savedSearches = savedSearches ?? [];
		});
		await this.#logViewerContext.getSavedSearches();
	}

	render() {
		return html` <umb-workspace-layout headline="Log Overview for today">
			<div id="logviewer-layout">
				<div id="info">
					<uui-box id="time-period" headline="Time Period">
						<uui-input type="date" label="From"></uui-input>
						<uui-input type="date" label="To"></uui-input>
					</uui-box>
					<uui-box id="errors" headline="Number of Errors"></uui-box>
					<uui-box id="level" headline="Log level"></uui-box>
					<uui-box id="types" headline="Log types"></uui-box>
				</div>
				<div id="saved-searches-container">
					<uui-box id="saved-searches" headline="Saved searches">
						${this._savedSearches.map((search) => html`<div>${search.name}</div>`)}
					</uui-box>
				</div>
				<div id="common-messages-container">
					<uui-box headline="Common Log Messages" id="saved-searches">
						${this._savedSearches.map((search) => html`<div>${search.name}</div>`)}
					</uui-box>
				</div>
			</div>
		</umb-workspace-layout>`;
	}
}

export default UmbLogViewerRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-root-workspace': UmbLogViewerRootWorkspaceElement;
	}
}
