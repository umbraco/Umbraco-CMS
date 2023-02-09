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
			${this._savedSearches.map((search) => html`<div>${search.name}</div>`)} bloblllo
		</umb-workspace-layout>`;
	}
}

export default UmbLogViewerRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-logviewer-root-workspace': UmbLogViewerRootWorkspaceElement;
	}
}
