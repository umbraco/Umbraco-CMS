import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import type { UmbWorkspaceNodeContext } from '../../../workspace-context/workspace-node.context';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import type { ContentProperty, ContentPropertyData, DocumentDetails, MediaDetails } from '@umbraco-cms/models';

import 'src/backoffice/components/content-property/content-property.element';
import 'src/backoffice/dashboards/media-management/dashboard-media-management.element';

@customElement('umb-workspace-view-content-collection')
export class UmbWorkspaceViewContentCollectionElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
			}
		`,
	];

	@state()
	_properties: ContentProperty[] = [];

	@state()
	_data: ContentPropertyData[] = [];

	@state()
	private _key = '';

	private _workspaceContext?: UmbWorkspaceNodeContext;

	constructor() {
		super();

		this.consumeContext('umbNodeContext', (nodeContext) => {
			this._workspaceContext = nodeContext;
			this._observeContent();
		});
	}

	private _observeContent() {
		if (!this._workspaceContext) return;

		this.observe<DocumentDetails | MediaDetails>(this._workspaceContext.data.pipe(distinctUntilChanged()), (content) => {
			this._properties = content.properties;
			this._data = content.data;
			this._key = content.key;
		});
	}

	render() {
		if (!this._key) return nothing;
		return html`<umb-dashboard-media-management .entityKey=${this._key}></umb-dashboard-media-management>`;
	}
}

export default UmbWorkspaceViewContentCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-content-collection': UmbWorkspaceViewContentCollectionElement;
	}
}
