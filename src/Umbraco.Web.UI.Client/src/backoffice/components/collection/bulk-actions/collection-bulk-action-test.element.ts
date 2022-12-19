import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import type { ManifestCollectionBulkAction } from '@umbraco-cms/models';

//TODO: Should be renamed to a specific usecase. eg. umb-media-collection-bulk-action
@customElement('umb-collection-bulk-action')
export class UmbCollectionBulkActionElement extends LitElement {
	static styles = [UUITextStyles, css``];

	public manifest?: ManifestCollectionBulkAction;

	render() {
		return html`<uui-button
			label=${ifDefined(this.manifest?.meta.label)}
			color="default"
			look="secondary"></uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-bulk-action': UmbCollectionBulkActionElement;
	}
}
